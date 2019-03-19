using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Configuration;
using JiraReporterCore.GSheets;
using PRJReports.Sin;

namespace PRJReports.GSheets
{
   public class SinnerSheet : WritableGoogleSheet<List<IEnumerable<Sinner>>>
   {
      public SinnerSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public override void Write(List<IEnumerable<Sinner>> data)
      {
         var dataToWrite = new List<IList<object>>();

         dataToWrite.AddRange(data.SelectMany(x => x.Select(y => y.ToRow())));

         Client.WriteToSheet(Settings.SheetName, dataToWrite);
      }
   }
}