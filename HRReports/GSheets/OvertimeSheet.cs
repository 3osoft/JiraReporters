using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Configuration;
using JiraReporterCore.GSheets;

namespace HRReports.GSheets
{
   public class OvertimeSheet : WritableGoogleSheet<List<Overtime>>
   {
      private readonly string _sheetPrefix;

      private static readonly string[] Headers =
      {
         "Login",
         "Meno",
         "Priezvisko",
         "Typ PP",
         "Stredisko",
         "#pracovných hodín v mesiaci",
         "Mesiac/Rok",
         "Absencie",
         "Rozdiel",
         "Reálne odpracované",
         "Nadčas"
      };

      public OvertimeSheet(GoogleSheetsSettings settings, string sheetPrefix) : base(settings)
      {
         _sheetPrefix = sheetPrefix;
      }

      public override void Write(List<Overtime> dataToWrite)
      {
         Client.MakeSheetExistAndVisible(GetSheetNameWithPrefix());
         Client.ClearSheet(GetSheetNameWithPrefix());
         List<IList<object>> data = new List<IList<object>> {GetHeader()};

         data.AddRange(dataToWrite.Select(GetRowDataFromUserData));

         Client.WriteToSheet(GetSheetNameWithPrefix(), data);
      }

      private string GetSheetNameWithPrefix()
      {
         return $"{_sheetPrefix}_{Settings.SheetName}";
      }


      private List<object> GetHeader()
      {
         return Headers.Cast<object>().ToList();
      }
      private List<object> GetRowDataFromUserData(Overtime data)
      {
         return new List<object>
         {
            data.Login,
            data.FirstName,
            data.LastName,
            data.ContractType,
            data.CostCenter,
            data.WorkHoursInMonth,
            $"{data.Month}/{data.Year}",
            data.Absences,
            data.WorkHoursInMonth - data.Absences,
            data.HoursWorked,
            data.OvertimeHours
         };
      }
   }
}