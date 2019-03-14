using System.Collections.Generic;

namespace JiraToolCheckFramework.Sin
{
   public class WorklogCountSinner : Sinner
   {
      public const int CountThreshold = 2;
      public override string SinString => "Malo worklogov (pocet worklogov, celkovy cas)";
      public int WorklogCount { get; set; }
      public decimal TotalHours { get; set; }

      public override IList<object> ToRow()
      {
         return new List<object>
         {
            SinDate.ToShortDateString(),
            SinnerLogin,
            SinString,
            WorklogCount,
            TotalHours
         };
      }
   }
}