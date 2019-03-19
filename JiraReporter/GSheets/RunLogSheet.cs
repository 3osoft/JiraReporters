using System;
using System.Collections.Generic;
using JiraReporter.Configuration;
using JiraReporter.Domain;

namespace JiraReporter.GSheets
{
   public class RunLogSheet : WritableGoogleSheet<ToolRun>
   {
      public RunLogSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public override void Write(ToolRun data)
      {
         var dataToWrite = new List<IList<object>> { new List<object> { data.RunStart.ToString("g"), data.RunEnd.ToString("g") } };
         Client.WriteToSheet(Settings.SheetName, dataToWrite);
      }
   }
}