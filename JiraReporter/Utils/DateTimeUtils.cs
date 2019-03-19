using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.JiraApi.Models;
using JiraReporterCore.Reporters;

namespace JiraReporterCore.Utils
{
   public class DateTimeUtils
   {
      public static DateTime GetLastWorkDay(List<PublicHoliday> holidays, DateTime fromDate)
      {
         var currentDate = fromDate.Date;

         while (IsNonWorkingDay(holidays, currentDate))
         {
            currentDate = currentDate.AddDays(-1);
         }

         return currentDate;
      }

      public static bool IsNonWorkingDay(List<PublicHoliday> holidays, DateTime date)
      {
         return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ||
                holidays.Select(x => x.Date).Contains(date.Date);
      }

      public static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
      {
         for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
         {
            yield return day;
         }
      }
   }
}