using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Database;
using JiraToolCheckFramework.JiraApi;
using JiraToolCheckFramework.Reporters;
using Newtonsoft.Json;

namespace JiraToolCheckFramework
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

         var shouldReportSinnersToday = !IsNonWorkingDay(DateTime.Now, publicHolidayReporter.Report());
         
         Logger.Info("Running for date range from {0} to {1}", from, till);

         JiraApiClient client = new JiraApiClient(config.JiraSettings);
         UserReporter userReporter = new UserReporter(config.UsersSheetSettings);

         WorklogsReporter fullHistoryWorklogsReporter = new WorklogsReporter(userReporter, client, new DateTime(2017, 1, 1),DateTime.Now);
         ProjectTimeSpentReporter projectTimeSpentReporter = new ProjectTimeSpentReporter(config.ProjectTimeSpentSheetSettings, fullHistoryWorklogsReporter);

         WorklogsReporter currentRangeWorklogsReporter = new WorklogsReporter(userReporter, client, from, till);

         AbsenceReporter absenceReporter = new AbsenceReporter(publicHolidayReporter, userReporter, client);

         AttendanceReporter attendanceReporter = new AttendanceReporter(userReporter, absenceReporter, currentRangeWorklogsReporter, from, till, config.AttendanceGridSheetSettings);

         projectTimeSpentReporter.Report();
         attendanceReporter.Report();

         if (shouldReportSinnersToday)
         {
            var dateOfSin = GetLastWorkDay(publicHolidayReporter, DateTime.Now.Date.AddDays(-1));
            SinnersReporter sinnersReporter = new SinnersReporter(userReporter, currentRangeWorklogsReporter, attendanceReporter, config, dateOfSin);
            sinnersReporter.Report();
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
               ctx.Absences.AddRange(absenceReporter.Report());
               ctx.Worklogs.AddRange(currentRangeWorklogsReporter.Report());
               ctx.Attendance.AddRange(attendanceReporter.Report());
               ctx.Users.AddRange(userReporter.Report());
               ctx.SaveChanges();
               transaction.Commit();
            }
         }

         DateTime runEndDateTime = DateTime.Now;

         ToolRunReporter runReporter = new ToolRunReporter(runStartDateTime, runEndDateTime, config.RunLogSheetSettings);
         runReporter.Report();

         Logger.Info("Tool run finished");
      }

      private static DateTime GetLastWorkDay(PublicHolidayReporter holidayReporter, DateTime fromDate)
      {
         var currentDate = fromDate.Date;
         var holidays = holidayReporter.Report();

         while (IsNonWorkingDay(currentDate, holidays))
         {
            currentDate = currentDate.AddDays(-1);
         }

         return currentDate;
      }

      private static bool IsNonWorkingDay(DateTime date, List<PublicHoliday> holidays)
      {
         return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ||
                holidays.Select(x => x.Date).Contains(date.Date);
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
