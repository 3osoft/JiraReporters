using System.Collections.Generic;
using System.Linq;
using JiraReporter.Configuration;
using JiraReporter.Reporters;
using JiraToolCheckFramework.Database;
using JiraToolCheckFramework.GSheets;

namespace JiraToolCheckFramework.Reporters
{
   public class UserReporter : BaseReporter<List<UserModel>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly UserSheet _userSheet;
      public UserReporter(GoogleSheetsSettings settings)
      {
         _userSheet = new UserSheet(settings);
      }

      public List<string> GetUserNames()
      {
         return Report().Select(x => x.UserName).ToList();
      }

      protected override List<UserModel> CalculateReportData()
      {
         Logger.Info("Getting users");
         var users = _userSheet.GetUsers();
         Logger.Info("Found {0} users", users.Count);
         return users;
      }
   }
}