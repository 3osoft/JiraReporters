using System.Collections.Generic;

namespace JiraToolCheckFramework.Sin
{
   public class LongWorklogSinner : Sinner
   {
      public const decimal LongWorklogThreshold = 4;
      public override string SinString => "Dlhy worklog (pocet hodin)";
      public decimal Hours { get; set; }

      public override IList<object> ToRow()
      {
         return new List<object>
         {
            SinDate.ToShortDateString(),
            SinnerLogin,
            SinString,
            $"{Hours:F2}h"
         };
      }

      public override string ToMailString()
      {
         return $"{SinnerLogin} - {Hours:F2}h";
      }
   }
}