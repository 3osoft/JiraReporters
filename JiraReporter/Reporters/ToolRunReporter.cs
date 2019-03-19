using System;
using JiraReporter.Configuration;
using JiraReporter.GSheets;

namespace JiraReporter.Reporters
{
   public class ToolRunReporter : BaseReporter<object>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly DateTime _runStart;
      private readonly DateTime _runEnd;
      private readonly GoogleSheetsSettings _settings;

      public ToolRunReporter(DateTime runStart, DateTime runEnd, GoogleSheetsSettings settings)
      {
         _runStart = runStart;
         _runEnd = runEnd;
         _settings = settings;
      }

      protected override object CalculateReportData()
      {
         var runLogSheet = new RunLogSheet(_settings);
         Logger.Info("Writng run log");
         runLogSheet.WriteLog(_runStart, _runEnd);

         return null;
      }
   }
}