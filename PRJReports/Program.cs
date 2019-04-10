using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using JiraReporterCore.Domain;
using JiraReporterCore.GSheets;
using JiraReporterCore.JiraApi;
using JiraReporterCore.Reporters;
using JiraReporterCore.Reporters.Users;
using JiraReporterCore.Reporters.Writer;
using JiraReporterCore.Utils;
using Newtonsoft.Json;
using PRJReports.Configuration;
using PRJReports.Database;
using PRJReports.Database.Mappers;
using PRJReports.Gmail;
using PRJReports.GSheets;
using PRJReports.Reporters;
using PRJReports.Sin;

namespace PRJReports
{
   class Program
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      public const string ConfigFilePath = "config.json";
      static void Main(string[] args)
      {
         Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));

         AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
         {
            Exception exception = eventArgs.ExceptionObject as Exception;
            Logger.Fatal(exception);

            ErrorNotificationGmail errorNotificationGmail = new ErrorNotificationGmail(config.ErrorGmailSettings);
            errorNotificationGmail.Write(exception);
         };

         DateTime runStartDateTime = DateTime.Now;
         
         DateTime from = config.DateFrom ?? new DateTime(DateTime.Now.Year, 1, 1) ;
         DateTime till = config.DateTo ?? new DateTime(DateTime.Now.Year, 12, 31);
         DateTime considerWorklogsFrom = DateTime.Now.AddMonths(-config.MonthsToLog).Date;

         PublicHolidayReporter publicHolidayReporter = new PublicHolidayReporter(config.PublicHolidayApiKey, Enumerable.Range(2016, till.Year - 2016 + 1).ToList());

         var shouldReportSinnersToday = !DateTimeUtils.IsNonWorkingDay(publicHolidayReporter.Report(), DateTime.Now);
         
         Logger.Info("Running for date range from {0} to {1}", from, till);

         JiraApiClient client = new JiraApiClient(config.JiraSettings);
         var rawUserDataReporter = new RawUserDataReporter(new RawUserDataSheet(config.UsersSheetSettings));
         var freshestUserDataReporter = new FreshestUserDataReporter(rawUserDataReporter);

         WorklogsReporter fullInitWorklogsReporter = new WorklogsReporter(freshestUserDataReporter, client, new DateTime(2017, 1, 1), considerWorklogsFrom);

         WorklogsReporter currentRangeWorklogsReporter = new WorklogsReporter(freshestUserDataReporter, client, from, till);

         JiraAbsenceReporter jiraAbsenceReporter = new JiraAbsenceReporter(freshestUserDataReporter, client);

         AbsenceReporter absenceReporter = new AbsenceReporter(publicHolidayReporter, jiraAbsenceReporter);

         AttendanceReporter attendanceReporter = new AttendanceReporter(freshestUserDataReporter, absenceReporter, currentRangeWorklogsReporter, from, till);

         var attendanceReportWriter = new ReportWriter<List<Attendance>>(attendanceReporter, new AttendanceGridSheet(config.AttendanceGridSheetSettings));
         attendanceReportWriter.Write();

         if (shouldReportSinnersToday)
         {
            var dateOfSin = DateTimeUtils.GetLastWorkDay(publicHolidayReporter.Report(), DateTime.Now.Date.AddDays(-1));
            var currentUsersReporter = new CurrentUsersReporter(freshestUserDataReporter);
            
            SinnersReporter sinnersReporter = new SinnersReporter(currentUsersReporter, currentRangeWorklogsReporter, attendanceReporter, dateOfSin);

            var gSheetSinnerReportWriter = new ReportWriter<List<IEnumerable<Sinner>>>(sinnersReporter, new SinnerSheet(config.SinnersSheetSettings));
            gSheetSinnerReportWriter.Write();

            var gmailSinnerReportWriter = new ReportWriter<List<IEnumerable<Sinner>>>(sinnersReporter, new SinnerGmail(config.SinnerNotifierGmailSettings, dateOfSin));
            gmailSinnerReportWriter.Write();
         }
         else
         {
            Logger.Info("Tool wont report sinners because it is weekend or holiday!");
         }

         Logger.Info("Cleaning and filling database");
         System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseIfModelChanges<JiraToolDbContext>());
         using (JiraToolDbContext ctx = new JiraToolDbContext())
         {
            using (var transaction = ctx.Database.BeginTransaction())
            {
               InsertInitialWorklogs(ctx, fullInitWorklogsReporter);
               ClearDatabase(ctx, considerWorklogsFrom);
               WorklogsReporter updateReporter = new WorklogsReporter(freshestUserDataReporter, client, GetDateTimeForUpdate(ctx, considerWorklogsFrom), DateTime.Now);
               ctx.Absences.AddRange(absenceReporter.Report().Select(AbsenceMapper.ToModel));
               ctx.Worklogs.AddRange(updateReporter.Report().Select(WorklogMapper.ToModel));
               ctx.Attendance.AddRange(attendanceReporter.Report().Select(AttendanceMapper.ToModel));
               ctx.Users.AddRange(freshestUserDataReporter.Report().Select(UserMapper.ToModel));
               ctx.SaveChanges();
               transaction.Commit();
            }
         }
         WorklogsFromDbReporter fullHistoryWorklogsReporter = new WorklogsFromDbReporter(freshestUserDataReporter, new JiraToolDbContext(), new DateTime(2017, 1, 1), DateTime.Now);
         ProjectTimeSpentReporter projectTimeSpentReporter = new ProjectTimeSpentReporter(fullHistoryWorklogsReporter);
         var projectTimeSpentWriter = new ReportWriter<Dictionary<string, decimal>>(projectTimeSpentReporter, new ProjectTimeSpentSheet(config.ProjectTimeSpentSheetSettings));
         projectTimeSpentWriter.Write();

         DateTime runEndDateTime = DateTime.Now;

         ToolRunReporter runReporter = new ToolRunReporter(runStartDateTime, runEndDateTime);
         ReportWriter<ToolRun> toolRunWriter = new ReportWriter<ToolRun>(runReporter, new RunLogSheet(config.RunLogSheetSettings));
         toolRunWriter.Write();

         Logger.Info("Tool run finished");
      }

      private static void ClearDatabase(JiraToolDbContext ctx, DateTime considerWorklogsFrom)
      {
         ctx.Absences.RemoveRange(ctx.Absences);
         ctx.Attendance.RemoveRange(ctx.Attendance);
         ctx.Users.RemoveRange(ctx.Users);
         ctx.Worklogs.RemoveRange(ctx.Worklogs.Where(log => log.Date >= considerWorklogsFrom));
      }

      private static void InsertInitialWorklogs(JiraToolDbContext ctx, WorklogsReporter fullInitReporter)
      {
         if (IsWorklogInitNecessary(ctx))
         {
            ctx.Worklogs.RemoveRange(ctx.Worklogs);
            ctx.Worklogs.AddRange(fullInitReporter.Report().Select(WorklogMapper.ToModel));
         }
      }

      private static bool IsWorklogInitNecessary(JiraToolDbContext ctx)
      {
         DateTime startPeriod = new DateTime(2017, 1, 31);
         return !ctx.Worklogs.Any(log => log.Date < startPeriod);
      }

      private static DateTime GetDateTimeForUpdate(JiraToolDbContext ctx, DateTime considerWorklogsFrom)
      {
         var result = ctx.Worklogs.OrderByDescending(log => log.Date).FirstOrDefault()?.Date.AddDays(1);
         if (result == null || result > considerWorklogsFrom)
         {
            result = considerWorklogsFrom;
         }

         return result.Value;
      }
   }
}
