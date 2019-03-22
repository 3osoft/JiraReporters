using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HRReports.Alerts;
using HRReports.Configuration;
using HRReports.Domain;
using HRReports.Gmail;
using HRReports.GSheets;
using HRReports.Reporters;
using JiraReporterCore.JiraApi;
using JiraReporterCore.JiraApi.Models;
using JiraReporterCore.Reporters;
using JiraReporterCore.Reporters.Writer;
using Newtonsoft.Json;

namespace HRReports
{
   class Program
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      public const string ConfigFilePath = "config.json";
      private const int HolidayYearStart = 2016;

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
         var publicHolidayReporter = new PublicHolidayReporter(config.PublicHolidayApiKey, Enumerable.Range(HolidayYearStart, currentDate.Year - HolidayYearStart + 2).ToList());

         CalculateMonthlyReports(currentUsersReporter, publicHolidayReporter, previousMonthStart, previousMonthEnd, config);
         CalculateMonthlyReports(currentUsersReporter, publicHolidayReporter, currentMonthStart, currentDate, config);
         

         SendAlerts(currentUsersReporter, publicHolidayReporter, currentDate, config);
      }

      private static void SendAlerts(CurrentUsersReporter currentUsersReporter,
         PublicHolidayReporter publicHolidayReporter, DateTime currentDate, Config config)
      {
         var userDataAlertReporter = new UserDataAlertReporter(currentUsersReporter, currentDate);
         JiraApiClient client = new JiraApiClient(config.JiraSettings);
         var userReporter = new UserReporter(config.CurrentUsersSheetSettings);
         var jiraAbsenceReporter = new JiraAbsenceReporter(userReporter, client);
         var absenceErrorReporter = new AbsenceErrorsReporter(jiraAbsenceReporter, publicHolidayReporter);

         var userDataAlertWriter = new ReportWriter<List<BaseAlert>>(userDataAlertReporter, new HrAlertGmail(config.HrAlertGmailSettings, currentDate));
         var absenceErrorWriter = new ReportWriter<List<AbsenceError>>(absenceErrorReporter, new AbsenceErrorGmail(config.AbsenceErrorGmailSettings));

         userDataAlertWriter.Write();
         absenceErrorWriter.Write();
      }

      private static void CalculateMonthlyReports(CurrentUsersReporter currentUsersReporter,
         PublicHolidayReporter holidayReporter, DateTime start, DateTime end, Config config)
      {
         var month = start.Month;
         var year = start.Year;

         var monthlySheetsPrefix = $"{year:D4}{month:D2}";

         JiraApiClient client = new JiraApiClient(config.JiraSettings);

         
         var userReporter = new UserReporter(config.CurrentUsersSheetSettings);
         var workHoursReporter = new MonthWorkHoursReporter(holidayReporter, month, year);
         var jiraAbsenceReporter = new JiraAbsenceReporter(userReporter, client);
         var absenceReporter = new AbsenceReporter(holidayReporter, jiraAbsenceReporter);
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
