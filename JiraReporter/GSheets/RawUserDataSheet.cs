using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Configuration;
using JiraReporterCore.Domain.Users;

namespace JiraReporterCore.GSheets
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
            RecordDate = TryParseDate(x[0]).Value,
            UserData = new UserData
            {
               Login = (string) x[1],
               Initials = (string) x[2],
               FirstName = (string) x[3],
               LastName = (string) x[4],
               IsTracking = TryParseBool(x[5]),
               Title = (string) x[6],
               Position = (string) x[7],
               StartDate = TryParseDate(x[8]),
               EndOfProbationPeriod = TryParseDate(x[9]),
               ContractValidityDate = TryParseDate(x[10]),
               TerminationDate = TryParseDate(x[11]),
               ContractType = (string) x[12],
               CostCenter = (string) x[13],
               PersonalDataConfirmation = TryParseBool(x[14]),
               Salary = TryParseDecimal(x[15]),
               Rate = TryParseDecimal(x[16]),
               PhoneNumber = (string) x[17],
               ICEPhoneNumber = (string) x[18],
               DateOfBirth = TryParseDate(x[19]),
               WorkAnniversaryDate = TryParseDate(x[20]),
               Benefit = (string) x[21],
               ContractOrAmendmentSignedOn = TryParseDate(x[22]),
               ContractOrAmendmentValidFrom = TryParseDate(x[23]),
               Note = (string) x[24]
            }
         }).ToList();

         return users;
      }

      private DateTime? TryParseDate(object o)
      {
         DateTime? result = null;

         var objectString = (string) o;

         if (DateTime.TryParse(objectString, out var value))
         {
            result = value;
         }

         return result;
      }

      private decimal? TryParseDecimal(object o)
      {
         decimal? result = null;

         var objectString = (string) o;

         if (decimal.TryParse(objectString, out var value))
         {
            result = value;
         }

         return result;
      }

      private bool? TryParseBool(object o)
      {
         bool? result = null;

         var objectString = (string)o;

         if (int.TryParse(objectString, out var value))
         {
            result = value == 1;
         }

         return result;
      }
   }
}