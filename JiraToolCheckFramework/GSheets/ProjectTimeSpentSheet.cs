using System.Collections.Generic;
using JiraToolCheckFramework.Configuration;

namespace JiraToolCheckFramework.GSheets
{
   public class ProjectTimeSpentSheet : GoogleSheet
   {
      private const string ProjectIdHeader = "Project";
      private const string TimeSpentHeader = "Hours spent";

      public ProjectTimeSpentSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public void WriteBudgetBurned(Dictionary<string, decimal> budgetBurnedDictionary)
      {
         _client.ClearSheet(_settings.SheetName);
         List<IList<object>> dataToWrite = new List<IList<object>> {new List<object> {ProjectIdHeader, TimeSpentHeader}};

         foreach (var projectBurn in budgetBurnedDictionary)
         {
            dataToWrite.Add(new List<object>
            {
               projectBurn.Key,
               projectBurn.Value
            });
         }

         _client.WriteToSheet(_settings.SheetName, dataToWrite);
      }

   }
}