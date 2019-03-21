using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Configuration;
using JiraReporterCore.GSheets;

namespace HRReports.GSheets
{
   public class CurrentUsersSheet : WritableGoogleSheet<List<UserData>>
   {
      private static readonly string[] Headers =
      {
         "Login",
         "Meno",
         "Priezvisko",
         "Pozícia",
         "Dátum nástupu",
         "Zmluvá platná do",
         "Typ PP",
         "Stredisko",
         "Súhlas os.údaje",
         "Mzda",
         "Odmena",
         "Telefónne číslo",
         "ICE kontakt",
         "Dátum narodenia",
         "Benefit",
         "Poznámky / Iné"
      };

      public CurrentUsersSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public override void Write(List<UserData> dataToWrite)
      {
         Client.ClearSheet(Settings.SheetName);

         List<IList<object>> data = new List<IList<object>> {GetHeader()};
         data.AddRange(dataToWrite.Select(GetRowDataFromUserData));
         Client.WriteToSheet(Settings.SheetName, data);
      }

      private List<object> GetHeader()
      {
         return Headers.Cast<object>().ToList();
      }

      private List<object> GetRowDataFromUserData(UserData data)
      {
         return new List<object>
         {
            data.Login,
            data.FirstName,
            data.LastName,
            data.Position,
            data.StartDate?.ToShortDateString(),
            data.ContractValidityDate?.ToShortDateString(),
            data.ContractType,
            data.CostCenter,
            data.PersonalDataConfirmation,
            data.Salary,
            data.Rate,
            data.PhoneNumber,
            data.ICEPhoneNumber,
            data.DateOfBirth,
            data.Benefit,
            data.Note
         };
      }
   }
}