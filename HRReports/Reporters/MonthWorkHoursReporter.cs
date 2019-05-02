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
      private readonly DateTime? _start;
      private readonly DateTime? _end;

      public MonthWorkHoursReporter(PublicHolidayReporter holidayReporter, int month, int year)
      {
         _holidayReporter = holidayReporter;
         _month = month;
         _year = year;
      }

      public MonthWorkHoursReporter(MonthWorkHoursReporter other, DateTime start, DateTime end)
      {
         _holidayReporter = other._holidayReporter;
         _start = start;
         _end = end;
      }

      protected override int CalculateReportData()
      {
         DateTime startDate = _start ?? new DateTime(_year, _month, 1);
         DateTime endDate = _end ?? startDate.AddMonths(1).AddDays(-1);

         string message = _start != null
            ? $"Calculating work hours from {_start} to {_end}"
            : $"Calculating work hours for month {_month} in year {_year}";
         Logger.Info(message);

         int result = DateTimeUtils.EachDay(startDate, endDate)
            .Count(x => !DateTimeUtils.IsNonWorkingDay(_holidayReporter.Report(), x)) * HoursInWorkDay;
         Logger.Info("Found {0} working hours", result);
         return result;
      }
   }
}