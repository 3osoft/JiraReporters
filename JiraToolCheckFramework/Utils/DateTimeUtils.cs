using System;
using System.Collections.Generic;

namespace JiraToolCheckFramework.Utils
{
   public class DateTimeUtils
   {
      public static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
      {
         for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
         {
            yield return day;
         }
      }
   }
}