using System;
using System.Collections.Generic;

namespace PRJReports.Sin
{
   public abstract class Sinner
   {
      public string SinnerLogin { get; set; }
      public DateTime SinDate { get; set; }
      public abstract string SinString { get; }
      public abstract IList<object> ToRow();
      public abstract string ToMailString();
   }
}