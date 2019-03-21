using System;
using System.Linq;
using JiraReporterCore.Reporters;
using JiraReporterCore.Utils;

namespace HRReports.Reporters
{
   public class MonthWorkHoursReporter : BaseReporter<int>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
      private const int HoursInWorkDay = 8;

      private readonly PublicHolidayReporter _holidayReporter;
      private readonly int _month;
      private readonly int _year;

      public MonthWorkHoursReporter(PublicHolidayReporter holidayReporter, int month, int year)
      {
         _holidayReporter = holidayReporter;
         _month = month;
         _year = year;
      }

      protected override int CalculateReportData()
      {
         var startDate = new DateTime(_year, _month, 1);
         var endDate = startDate.AddMonths(1).AddDays(-1);

         Logger.Info("Calculating work hours for month {0} in year {1}", _month, _year);

         var result = DateTimeUtils.EachDay(startDate, endDate)
            .Count(x => !DateTimeUtils.IsNonWorkingDay(_holidayReporter.Report(), x)) * HoursInWorkDay;
         Logger.Info("Found {0} working hours", result);
         return result;
      }
   }
}