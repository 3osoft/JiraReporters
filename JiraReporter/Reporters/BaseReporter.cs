using System;

namespace JiraReporterCore.Reporters
{
   public abstract class BaseReporter<T>
   {
      protected readonly Lazy<T> LazyReportDataReferenceLazy;

      protected BaseReporter()
      {
         LazyReportDataReferenceLazy = new Lazy<T>(CalculateReportData);
      }
      protected abstract T CalculateReportData();

      public T Report()
      {
         return LazyReportDataReferenceLazy.Value;
      }
   }
}