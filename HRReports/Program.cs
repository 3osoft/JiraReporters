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
         CalculateMonthlyReports(currentUsersReporter, currentMonthStart, currentDate, config);
         
      }

      private static void CalculateMonthlyReports(CurrentUsersReporter currentUsersReporter, DateTime start, DateTime end, Config config)
      {
         var month = start.Month;
         var year = start.Year;

         var monthlySheetsPrefix = $"{year:D4}{month:D2}";

         var yearsToReport = new List<int> {start.Year};
         if (start.Year != end.Year)
         {
            yearsToReport.Add(end.Year);
         }

         JiraApiClient client = new JiraApiClient(config.JiraSettings);

         
         var userReporter = new UserReporter(config.CurrentUsersSheetSettings);
         var publicHolidayReporter = new PublicHolidayReporter(config.PublicHolidayApiKey, yearsToReport);
         var workHoursReporter = new MonthWorkHoursReporter(publicHolidayReporter, month, year);
         var jiraAbsenceReporter = new JiraAbsenceReporter(userReporter, client);
         var absenceReporter = new AbsenceReporter(publicHolidayReporter, jiraAbsenceReporter);
         var worklogsReporter = new WorklogsReporter(userReporter, client, start, end);
         var attendanceReporter = new AttendanceReporter(userReporter, absenceReporter, worklogsReporter, start, end);

         var overtimeReporter = new OvertimeReporter(attendanceReporter, currentUsersReporter, workHoursReporter, year, month);
         var salaryDataReporter = new SalaryDataReporter(currentUsersReporter, attendanceReporter, jiraAbsenceReporter, year, month);
         var foodStampReporter = new FoodStampReporter(workHoursReporter, absenceReporter, currentUsersReporter, year, month);

         var overtimeWriter = new ReportWriter<List<Overtime>>(overtimeReporter, new OvertimeSheet(config.OvertimeSheetSettings, monthlySheetsPrefix));
         var salaryDataWriter = new ReportWriter<List<SalaryData>>(salaryDataReporter, new SalaryDataSheet(config.SalaryDataSheetSettings, monthlySheetsPrefix));
         var foodStampWriter = new ReportWriter<List<FoodStampData>>(foodStampReporter, new FoodStampSheet(config.FoodStampSheetSettings, monthlySheetsPrefix));

         overtimeWriter.Write();
         salaryDataWriter.Write();
         foodStampWriter.Write();
      }
   }
}
