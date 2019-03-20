using System.Collections.Generic;

namespace PRJReports.Sin
{
   public class NoTimeTrackedSinner : Sinner
   {
      public override string SinString => "zatrackovany cas 0h";
      public override IList<object> ToRow()
      {
         return new List<object>
         {
            SinDate.ToShortDateString(),
            SinnerLogin,
            SinString
         };
      }

      public override string ToMailString()
      {
         return $"{SinnerLogin}";
      }
   }
}