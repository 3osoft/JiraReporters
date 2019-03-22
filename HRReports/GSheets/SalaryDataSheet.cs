using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Configuration;

namespace HRReports.GSheets
{
   public class SalaryDataSheet : MonthlyHrReportSheet<List<SalaryData>>
   {
      private static readonly string[] Headers =
      {
         "Mesiac/Rok",
         "Login",
         "Meno",
         "Priezvisko",
         "Titul",
         "Dátum nástupu",
         "Mzda",
         "Benefit ",
         "Dovolenka (deň)",
         "Návšteva lekára (hod.)",
         "Doprovod lekár (hod.)",
         "PN (dni)",
         "Odmena/bonus",
         "Poznámky"
      };

      public SalaryDataSheet(GoogleSheetsSettings settings, string sheetPrefix) : base(settings, sheetPrefix)
      {
      }

      protected override List<IList<object>> GetRowDataToWrite(List<SalaryData> dataToWrite)
      {
         return dataToWrite.Select(GetRowDataFromSalaryData).ToList();
      }

      protected override List<object> GetHeaders()
      {
         return Headers.Cast<object>().ToList();
      }

      private IList<object> GetRowDataFromSalaryData(SalaryData data)
      {
         return new List<object>
         {
            $"{data.Month:D2}/{data.Year:D4}",
            data.Login,
            data.FirstName,
            data.LastName,
            data.Title,
            data.StartDate?.ToShortDateString(),
            data.Salary,
            data.Benefit,
            data.VacationDays,
            data.DoctorHours,
            data.DoctorFamilyHours,
            data.IllnessDays
         };
      }
   }
}