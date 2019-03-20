using System.Collections.Generic;
using System.Linq;

namespace PRJReports.Sin
{
   public class SinsEvaluator
   {
      public bool CanWeHaveAMeme(List<IEnumerable<Sinner>> sinners)
      {
         return !sinners.SelectMany(x => x).Any(x => x is NoTimeTrackedSinner);
      }
   }
}