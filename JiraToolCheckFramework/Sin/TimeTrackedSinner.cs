using System.Collections.Generic;

namespace JiraToolCheckFramework.Sin
{
   public class TimeTrackedSinner : Sinner
   {
      public const decimal LowHoursThreshold = 6;
      public const decimal HighHoursThreshold = 10;

      public override string SinString => TotalHours < LowHoursThreshold ? "Malo hodin (trackovane, absencia, celkom)" : "Vela hodin (trackovane, absencia, celkom)";

      public decimal TimeTracked { get; set; }
      public decimal Absence { get; set; }
      public decimal TotalHours { get; set; }

      public override IList<object> ToRow()
      {
         return new List<object>
         {
            SinDate.ToShortDateString(),
            SinnerLogin,
            SinString,
            TimeTracked,
            Absence,
            TotalHours
         };
      }
   }
}