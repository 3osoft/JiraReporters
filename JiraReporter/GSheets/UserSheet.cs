using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Configuration;
using JiraReporterCore.Domain;

namespace JiraReporterCore.GSheets
{
   public class UserSheet : GoogleSheet
   {
      private const int UserSheetRowsToSkip = 1;
      private const int UserSheetLoginColumnIndex = 0;
      private const int UserSheetInitialsColumnIndex = 1;
      private const int UserIsTrackingColumnIndex = 4;

      public UserSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public List<User> GetUsers()
      {
         IList<IList<object>> userSheetData = Client.GetSheetData(Settings.SheetName);
         var users = userSheetData.Skip(UserSheetRowsToSkip).Select(x => new User
         {
            UserName = (string) x[UserSheetLoginColumnIndex],
            Initials = (string) x[UserSheetInitialsColumnIndex],
            IsTracking = int.Parse((string) x[UserIsTrackingColumnIndex]) == 1
         }).ToList();
         return users;
      }
   }
}