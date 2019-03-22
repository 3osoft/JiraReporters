using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Configuration;

namespace HRReports.GSheets
{
   public class FoodStampSheet : MonthlyHrReportSheet<List<FoodStampData>>
   {
      private static readonly string[] Headers =
      {
         "Mesiac/Rok",
         "Meno",
         "Priezvisko",
         "Titul",
         "Nárok (# prac. dní v mesiaci)",
         "Úprava (strhnutie za absencie)",
         "# vydaných lístkov"
      };

      public FoodStampSheet(GoogleSheetsSettings settings, string sheetPrefix) : base(settings, sheetPrefix)
      {
      }

      protected override List<IList<object>> GetRowDataToWrite(List<FoodStampData> dataToWrite)
      {
         //TODO maybe implement the footer and other columns!
         return dataToWrite.Select(GetRowDataFromFoodStampData).ToList();
      }

      protected override List<object> GetHeaders()
      {
         return Headers.Cast<object>().ToList();
      }

      private IList<object> GetRowDataFromFoodStampData(FoodStampData data)
      {
         return new List<object>
         {
            $"{data.Month:D2}/{data.Year:D4}",
            data.FirstName,
            data.LastName,
            data.Title,
            data.FoodStampCountEntitlement,
            data.AdjustmentForAbsences,
            data.IssuesFoodStampCount
         };
      }
   }
}