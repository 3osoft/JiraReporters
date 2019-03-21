using System;
using System.Collections.Generic;
using System.IO;
using HRReports.Configuration;
using HRReports.Domain;
using HRReports.GSheets;
using HRReports.Reporters;
using JiraReporterCore.JiraApi;
using JiraReporterCore.Reporters;
using JiraReporterCore.Reporters.Writer;
using Newtonsoft.Json;

namespace HRReports
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

         var currentDate = DateTime.Now;
         var currentMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
         var previousMonthStart = currentMonthStart.AddMonths(-1);
         var previousMonthEnd = currentMonthStart.AddSeconds(-1);

         //TODO first hide all tabs in the HR sheet report
         
         var rawUserDataReporter = new RawUserDataReporter(new RawUserDataSheet(config.RawUsersSheetSettings));
         var currentUsersReporter = new CurrentUsersReporter(rawUserDataReporter);

         ReportWriter<List<UserData>> userDataWriter = new ReportWriter<List<UserData>>(currentUsersReporter, new CurrentUsersSheet(config.CurrentUsersSheetSettings));
         userDataWriter.Write();

         CalculateMonthlyReports(currentUsersReporter, previousMonthStart, previousMonthEnd, config);
         
      }

      private static void CalculateMonthlyReports(CurrentUsersReporter currentUsersReporter, DateTime start, DateTime end, Config config)
      {
         var yearsToReport = new List<int> {start.Year};
         if (start.Year != end.Year)
         {
            yearsToReport.Add(end.Year);
         }

         JiraApiClient client = new JiraApiClient(config.JiraSettings);

         
         var userReporter = new UserReporter(config.CurrentUsersSheetSettings);
         var publicHolidayReporter = new PublicHolidayReporter(config.PublicHolidayApiKey, yearsToReport);
         var workHoursReporter = new MonthWorkHoursReporter(publicHolidayReporter, start.Month, start.Year);
         var absenceReporter = new AbsenceReporter(publicHolidayReporter, userReporter, client);
         var worklogsReporter = new WorklogsReporter(userReporter, client, start, end);
         var attendanceReporter = new AttendanceReporter(userReporter, absenceReporter, worklogsReporter, start, end);
         var overtimeReporter = new OvertimeReporter(attendanceReporter, currentUsersReporter, workHoursReporter, start.Year, start.Month);

         var overtimeWriter = new ReportWriter<List<Overtime>>(overtimeReporter, new OvertimeSheet(config.OvertimeSheetSettings, $"{start.Year:D4}{start.Month:D2}"));

         overtimeWriter.Write();
      }
   }
}
