using System.Collections.Generic;
using System.Linq;
using JiraReporter.Configuration;
using JiraReporter.GSheets;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Sin;

namespace JiraToolCheckFramework.GSheets
{
   public class SinnersSheet : GoogleSheet
   {
      public SinnersSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public void WriteSinners(List<IEnumerable<Sinner>> sinners)
      {
         var dataToWrite = new List<IList<object>>();

         dataToWrite.AddRange(sinners.SelectMany(x => x.Select(y => y.ToRow())));

         Client.WriteToSheet(Settings.SheetName, dataToWrite);
      }
   }
}