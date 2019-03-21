using System;
using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Configuration;
using JiraReporterCore.GSheets;

namespace HRReports.GSheets
{
   public class RawUserDataSheet : GoogleSheet
   {
      private const int UserSheetRowsToSkip = 1;

      public RawUserDataSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public List<RawUserData> GetRawUserData()
      {
         IList<IList<object>> userSheetData = Client.GetSheetData(Settings.SheetName);
         var users = userSheetData.Skip(UserSheetRowsToSkip).Select(x => new RawUserData
         {
            RecordDate = (DateTime) x[0],
            UserData = new UserData
            {
               Login = (string) x[1],
               Initials = (string) x[2],
               FirstName = (string) x[3],
               LastName = (string) x[4],
               IsTracking = int.Parse((string)x[5]) == 1,
               Title = (string) x[6],
               Position = (string) x[7],
               StartDate = DateTime.Parse((string)x[8]),
               EndOfProbationPeriod = DateTime.Parse((string)x[9]),
               ContractValidityDate = DateTime.Parse((string)x[10]),
               TerminationDate = DateTime.Parse((string)x[11]),
               ContractType = (string) x[12],
               CostCenter = (string) x[13],
               PersonalDataConfirmation = int.Parse((string)x[14]) == 1,
               Salary = (decimal?) x[15],
               Rate = (decimal?) x[16],
               PhoneNumber = (string) x[17],
               ICEPhoneNumber = (string) x[18],
               DateOfBirth = DateTime.Parse((string)x[19]),
               Benefit = (string) x[20],
               Note = (string) x[21]
            }
         }).ToList();

         return users;
      }
   }
}