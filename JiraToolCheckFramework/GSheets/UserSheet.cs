using System.Collections.Generic;
using System.Linq;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Database;
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

      public List<UserModel> GetUsers()
      {
         IList<IList<object>> userSheetData = _userGSheet.GetSheetData(_settings.UserSheetName);
         var users = userSheetData.Skip(_settings.UserSheetRowsToSkip).Select(x => new UserModel
         {
            UserName = (string) x[_settings.UserSheetLoginColumnIndex],
            Initials = (string) x[_settings.UserSheetInitialsColumnIndex]
         }).ToList();
         return users;
      }
   }
}