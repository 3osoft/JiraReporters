using System.Collections.Generic;
using JiraReporterCore.Configuration;
using JiraReporterCore.Domain;

namespace JiraReporterCore.GSheets
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