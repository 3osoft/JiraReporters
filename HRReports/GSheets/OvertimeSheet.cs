using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Configuration;

namespace HRReports.GSheets
{
   public class OvertimeSheet : MonthlyHrReportSheet<List<Overtime>>
   {
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

      public OvertimeSheet(GoogleSheetsSettings settings, string sheetPrefix) : base(settings, sheetPrefix)
      {
      }

      protected override List<IList<object>> GetRowDataToWrite(List<Overtime> dataToWrite)
      {
         return dataToWrite.Select(GetRowDataFromUserData).ToList();
      }

      protected override List<object> GetHeaders()
      {
         return Headers.Cast<object>().ToList();
      }
      
      private IList<object> GetRowDataFromUserData(Overtime data)
      {
         return new List<object>
         {
            data.Login,
            data.FirstName,
            data.LastName,
            data.ContractType,
            data.CostCenter,
            data.WorkHoursInMonth,
            $"{data.Month:D2}/{data.Year:D4}",
            data.Absences,
            data.WorkHoursInMonth - data.Absences,
            data.HoursWorked,
            data.OvertimeHours
         };
      }
   }
}