using System.Collections.Generic;
using System.Linq;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Database;

namespace JiraToolCheckFramework.GSheets
{
   public class UserSheet : GoogleSheet
   {
      private const int UserSheetRowsToSkip = 1;
      private const int UserSheetLoginColumnIndex = 1;
      private const int UserSheetInitialsColumnIndex = 2;
      private const int UserIsTrackingColumnIndex = 5;

      public UserSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public List<UserModel> GetUsers()
      {
         IList<IList<object>> userSheetData = _client.GetSheetData(_settings.SheetName);
         var users = userSheetData.Skip(UserSheetRowsToSkip).Select(x => new UserModel
         {
            UserName = (string) x[UserSheetLoginColumnIndex],
            Initials = (string) x[UserSheetInitialsColumnIndex],
            IsTracking = int.Parse((string) x[UserIsTrackingColumnIndex]) == 1
         }).ToList();
         return users;
      }
   }
}