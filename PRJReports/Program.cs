using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using JiraReporterCore.Domain;
using JiraReporterCore.GSheets;
using JiraReporterCore.JiraApi;
using JiraReporterCore.Reporters;
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
         AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
         {
            Logger.Fatal(eventArgs.ExceptionObject as Exception);
         };

         Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));

         DateTime runStartDateTime = DateTime.Now;
         
         DateTime from = config.DateFrom ?? new DateTime(DateTime.Now.Year, 1, 1) ;
         DateTime till = config.DateTo ?? new DateTime(DateTime.Now.Year, 12, 31);

         PublicHolidayReporter publicHolidayReporter = new PublicHolidayReporter(config.PublicHolidayApiKey, Enumerable.Range(2016, till.Year - 2016 + 1).ToList());

         var shouldReportSinnersToday = !DateTimeUtils.IsNonWorkingDay(publicHolidayReporter.Report(), DateTime.Now);
         
         Logger.Info("Running for date range from {0} to {1}", from, till);

         JiraApiClient client = new JiraApiClient(config.JiraSettings);
         UserReporter userReporter = new UserReporter(config.UsersSheetSettings);

         WorklogsReporter fullHistoryWorklogsReporter = new WorklogsReporter(userReporter, client, new DateTime(2017, 1, 1),DateTime.Now);
         ProjectTimeSpentReporter projectTimeSpentReporter = new ProjectTimeSpentReporter(fullHistoryWorklogsReporter);

         WorklogsReporter currentRangeWorklogsReporter = new WorklogsReporter(userReporter, client, from, till);

         AbsenceReporter absenceReporter = new AbsenceReporter(publicHolidayReporter, userReporter, client);

         AttendanceReporter attendanceReporter = new AttendanceReporter(userReporter, absenceReporter, currentRangeWorklogsReporter, from, till);

         var attendanceReportWriter = new ReportWriter<List<Attendance>>(attendanceReporter, new AttendanceGridSheet(config.AttendanceGridSheetSettings));
         attendanceReportWriter.Write();

         var projectTimeSpentWriter = new ReportWriter<Dictionary<string, decimal>>(projectTimeSpentReporter, new ProjectTimeSpentSheet(config.ProjectTimeSpentSheetSettings));
         projectTimeSpentWriter.Write();

         if (shouldReportSinnersToday)
         {
            var dateOfSin = DateTimeUtils.GetLastWorkDay(publicHolidayReporter.Report(), DateTime.Now.Date.AddDays(-1));
            SinnersReporter sinnersReporter = new SinnersReporter(userReporter, currentRangeWorklogsReporter, attendanceReporter, dateOfSin);

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
               ClearDatabase(ctx);
               ctx.Absences.AddRange(absenceReporter.Report().Select(AbsenceMapper.ToModel));
               ctx.Worklogs.AddRange(currentRangeWorklogsReporter.Report().Select(WorklogMapper.ToModel));
               ctx.Attendance.AddRange(attendanceReporter.Report().Select(AttendanceMapper.ToModel));
               ctx.Users.AddRange(userReporter.Report().Select(UserMapper.ToModel));
               ctx.SaveChanges();
               transaction.Commit();
            }
         }

         DateTime runEndDateTime = DateTime.Now;

         ToolRunReporter runReporter = new ToolRunReporter(runStartDateTime, runEndDateTime);
         ReportWriter<ToolRun> toolRunWriter = new ReportWriter<ToolRun>(runReporter, new RunLogSheet(config.RunLogSheetSettings));
         toolRunWriter.Write();

         Logger.Info("Tool run finished");
      }

      private static void ClearDatabase(JiraToolDbContext ctx)
      {
         ctx.Absences.RemoveRange(ctx.Absences);
         ctx.Worklogs.RemoveRange(ctx.Worklogs);
         ctx.Attendance.RemoveRange(ctx.Attendance);
         ctx.Users.RemoveRange(ctx.Users);
      }
   }
}
