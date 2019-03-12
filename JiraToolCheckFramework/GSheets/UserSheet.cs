using System.Collections.Generic;
using System.Linq;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.GSheets.FinanceSimulator.Utilities;

namespace JiraToolCheckFramework.GSheets
{
   public class UserSheet
   {
      private readonly GSheet _userGSheet;
      private readonly GoogleSheetsSettings _settings;

      public UserSheet(GoogleSheetsSettings settings)
      {
         _settings = settings;
         _userGSheet = new GSheet(_settings.GoogleSheetId);
      }

      public List<string> GetUsers()
      {
         IList<IList<object>> userSheetData = _userGSheet.GetSheetData(_settings.UserSheetName);
         var users = userSheetData.Skip(_settings.UserSheetRowsToSkip).Select(x => x[_settings.UserSheetLoginColumnIndex]).Cast<string>().ToList();
         return users;
      }
   }
}