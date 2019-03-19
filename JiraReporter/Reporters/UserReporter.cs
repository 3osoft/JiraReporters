using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Configuration;
using JiraReporterCore.Domain;
using JiraReporterCore.GSheets;

namespace JiraReporterCore.Reporters
{
   public class UserReporter : BaseReporter<List<User>>
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

      protected override List<User> CalculateReportData()
      {
         Logger.Info("Getting users");
         var users = _userSheet.GetUsers();
         Logger.Info("Found {0} users", users.Count);
         return users;
      }
   }
}