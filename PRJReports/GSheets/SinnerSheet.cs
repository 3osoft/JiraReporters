using System.Collections.Generic;
using System.Linq;
using JiraReporter.Configuration;
using JiraReporter.GSheets;
using JiraToolCheckFramework.Sin;

namespace JiraToolCheckFramework.GSheets
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