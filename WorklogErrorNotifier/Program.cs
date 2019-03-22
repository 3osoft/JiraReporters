using System;
using System.Collections.Generic;
using System.IO;
using JiraReporterCore.JiraApi;
using JiraReporterCore.Reporters;
using JiraReporterCore.Reporters.Writer;
using JiraReporterCore.Utils;
using Newtonsoft.Json;
using PRJReports.Configuration;
using PRJReports.Gmail;
using PRJReports.GSheets;
using PRJReports.Reporters;
using PRJReports.Sin;

namespace WorklogErrorNotifier
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

         //DailyNotification(config);

         FillSheetForDateRange(config);
      }

      private static void DailyNotification(Config config)
      {
         throw new NotImplementedException();
      }

      private static void FillSheetForDateRange(Config config)
      {
         var startDate = new DateTime(2019, 03, 1);
         var endDate = new DateTime(2019, 03, 7);

         var client = new JiraApiClient(config.JiraSettings);
         var userReporter = new UserReporter(config.UsersSheetSettings);
         var publicHolidayReporter = new PublicHolidayReporter(config.PublicHolidayApiKey, new List<int> {2019});
         var jiraAbsenceReporter = new JiraAbsenceReporter(userReporter, client);
         var absenceReporter = new AbsenceReporter(publicHolidayReporter, jiraAbsenceReporter);

         foreach (var date in DateTimeUtils.EachDay(startDate, endDate))
         {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
               Logger.Info($"Notifying {date:d}");

               var currentRangeWorklogsReporter =
                  new WorklogsReporter(userReporter, client, date.Date, date.Date.AddDays(1).AddSeconds(-1));
               var attendanceReporter = new AttendanceReporter(userReporter, absenceReporter, currentRangeWorklogsReporter,
                  date.Date, date.Date.AddDays(1).AddSeconds(-1));

               SinnersReporter sinnersReporter =
                  new SinnersReporter(userReporter, currentRangeWorklogsReporter, attendanceReporter, date);

               var gSheetSinnerReportWriter =
                  new ReportWriter<List<IEnumerable<Sinner>>>(sinnersReporter, new SinnerSheet(config.SinnersSheetSettings));
               gSheetSinnerReportWriter.Write();

               var gmailSinnerReportWriter = new ReportWriter<List<IEnumerable<Sinner>>>(sinnersReporter,
                  new SinnerGmail(config.SinnerNotifierGmailSettings, date));
               gmailSinnerReportWriter.Write();
            }
         }
      }
   }
}
