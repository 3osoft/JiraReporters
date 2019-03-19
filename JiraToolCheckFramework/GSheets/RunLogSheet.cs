using System;
using System.Collections.Generic;
using JiraReporter.Configuration;
using JiraReporter.GSheets;
using JiraToolCheckFramework.Configuration;

namespace JiraToolCheckFramework.GSheets
{
   public class RunLogSheet : GoogleSheet
   {
      public RunLogSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public void WriteLog(DateTime start, DateTime end)
      {
         var dataToWrite = new List<IList<object>> {new List<object> {start.ToString("g"), end.ToString("g")}};
         Client.WriteToSheet(Settings.SheetName, dataToWrite);
      }
   }
}