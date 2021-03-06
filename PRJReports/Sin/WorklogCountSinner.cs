﻿using System.Collections.Generic;

namespace PRJReports.Sin
{
   public class WorklogCountSinner : Sinner
   {
      public const int CountThreshold = 2;
      public override string SinString => "malo worklogov (pocet worklogov, celkovy cas)";
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
            $"{TotalHours:F2}h"
         };
      }

      public override string ToMailString()
      {
         return $"{SinnerLogin} - {WorklogCount}, {TotalHours:F2}h";
      }
   }
}