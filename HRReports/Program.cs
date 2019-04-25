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
using JiraReporterCore.Domain.Users;
using JiraReporterCore.GSheets;
using JiraReporterCore.JiraApi;
using JiraReporterCore.JiraApi.Models;
using JiraReporterCore.Reporters;
using JiraReporterCore.Reporters.Users;
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

         var date = DateTime.Now;
         var month = new DateTime(date.Year, date.Month, 1);
         
         //TODO first hide all tabs in the HR sheet report
         
         var rawUserDataReporter = new RawUserDataReporter(new RawUserDataSheet(config.RawUsersSheetSettings));
         var freshestUserDataReporter = new FreshestUserDataReporter(rawUserDataReporter);

         var currentUsersReporter = new CurrentUsersReporter(freshestUserDataReporter);

         ReportWriter<List<UserData>> userDataWriter = new ReportWriter<List<UserData>>(currentUsersReporter, new CurrentUsersSheet(config.CurrentUsersSheetSettings));
         userDataWriter.Write();
         var publicHolidayReporter = new PublicHolidayReporter(config.PublicHolidayApiKey, Enumerable.Range(HolidayYearStart, month.Year - HolidayYearStart + 2).ToList());

         CalculateMonthlyReports(freshestUserDataReporter, publicHolidayReporter, month, config);

         SendAlerts(freshestUserDataReporter, publicHolidayReporter, date, config);
      }

      private static void SendAlerts(FreshestUserDataReporter freshestUserDataReporter,
         PublicHolidayReporter publicHolidayReporter, DateTime currentDate, Config config)
      {
         var currentUsersReporter = new CurrentUsersReporter(freshestUserDataReporter);
         var userDataAlertReporter = new UserDataAlertReporter(currentUsersReporter, currentDate);
         JiraApiClient client = new JiraApiClient(config.JiraSettings);
         var jiraAbsenceReporter = new JiraAbsenceReporter(freshestUserDataReporter, client);
         var absenceErrorReporter = new AbsenceErrorsReporter(jiraAbsenceReporter, publicHolidayReporter);

         var userDataAlertWriter = new ReportWriter<List<BaseAlert>>(userDataAlertReporter, new HrAlertGmail(config.HrAlertGmailSettings, currentDate));
         var absenceErrorWriter = new ReportWriter<List<AbsenceError>>(absenceErrorReporter, new AbsenceErrorGmail(config.AbsenceErrorGmailSettings));

         userDataAlertWriter.Write();
         absenceErrorWriter.Write();
      }

      private static void CalculateMonthlyReports(FreshestUserDataReporter freshestUserDataReporter,
         PublicHolidayReporter holidayReporter, DateTime date, Config config)
      {
         JiraApiClient client = new JiraApiClient(config.JiraSettings);
         var jiraAbsenceReporter = new JiraAbsenceReporter(freshestUserDataReporter, client);
         var absenceReporter = new AbsenceReporter(holidayReporter, jiraAbsenceReporter);

         var previousDate = date.AddMonths(-1);
         var previousDateEnd = date.AddSeconds(-1);
         var previousDateYear = previousDate.Year;
         var previousDateMonth = previousDate.Month;

         var previousDateSheetsPrefix = $"{previousDateYear:D4}{previousDateMonth:D2}";

         var previousDateActiveUsersReporter = new UsersActiveInMonthReporter(
            freshestUserDataReporter, 
            previousDate, 
            previousDateEnd);         

         var previousDateWorkHoursReporter = new MonthWorkHoursReporter(
            holidayReporter, 
            previousDateMonth, 
            previousDateYear);
         
         var previousDateWorklogsReporter = new WorklogsReporter(
            freshestUserDataReporter, 
            client, 
            previousDate,
            previousDateEnd);

         var attendanceReporter = new AttendanceReporter(
            freshestUserDataReporter, 
            absenceReporter, 
            previousDateWorklogsReporter, 
            previousDate, 
            previousDateEnd);

         var overtimeReporter = new OvertimeReporter(
            attendanceReporter, 
            previousDateActiveUsersReporter, 
            previousDateWorkHoursReporter, 
            previousDateYear, 
            previousDateMonth);
         var overtimeWriter = new ReportWriter<List<Overtime>>(
            overtimeReporter, 
            new OvertimeSheet(config.OvertimeSheetSettings, previousDateSheetsPrefix));
         overtimeWriter.Write();

         var salaryDataReporter = new SalaryDataReporter(
            previousDateActiveUsersReporter, 
            attendanceReporter, 
            jiraAbsenceReporter, 
            previousDateYear, 
            previousDateMonth);
         var salaryDataWriter = new ReportWriter<List<SalaryData>>(
            salaryDataReporter, 
            new SalaryDataSheet(config.SalaryDataSheetSettings, previousDateSheetsPrefix));
         salaryDataWriter.Write();

         var dateEnd = date.AddMonths(1).AddSeconds(-1);
         var month = date.Month;
         var year = date.Year;
         var foodStampSheetsPrefix = $"{year:D4}{month:D2}";

         var foodStampWorkHoursReporter = new MonthWorkHoursReporter(holidayReporter, month, year);
         var foodStampActiveUsersReporter = new UsersActiveInMonthReporter(freshestUserDataReporter, date, dateEnd);

         var foodStampReporter = new FoodStampReporter(
            foodStampWorkHoursReporter, 
            absenceReporter, 
            foodStampActiveUsersReporter, 
            year, 
            month,
            previousDateYear,
            previousDateMonth);

         var foodStampWriter = new ReportWriter<List<FoodStampData>>(
            foodStampReporter, 
            new FoodStampSheet(config.FoodStampSheetSettings, foodStampSheetsPrefix));

         foodStampWriter.Write();
      }
   }
}
