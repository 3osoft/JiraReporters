using System;
using JiraReporterCore.Domain;

namespace JiraReporterCore.Reporters
{
   public class ToolRunReporter : BaseReporter<ToolRun>
   {
      private readonly DateTime _runStart;
      private readonly DateTime _runEnd;

      public ToolRunReporter(DateTime runStart, DateTime runEnd)
      {
         _runStart = runStart;
         _runEnd = runEnd;
      }

      protected override ToolRun CalculateReportData()
      {
         return new ToolRun
         {
            RunStart = _runStart,
            RunEnd = _runEnd
         };
      }
   }
}