using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain.Users;

namespace JiraReporterCore.Reporters.Users
{
   public class FreshestUserDataReporter : BaseReporter<List<UserData>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly RawUserDataReporter _rawUserDataReporter;

      public FreshestUserDataReporter(RawUserDataReporter rawUserDataReporter)
      {
         _rawUserDataReporter = rawUserDataReporter;
      }

      protected override List<UserData> CalculateReportData()
      {
         Logger.Info("Getting Freshest users");
         var groupedUserData = _rawUserDataReporter.Report().GroupBy(u => new
         {
            u.UserData.Login
         });

         var freshestUserData = groupedUserData.Select(x => x.OrderByDescending(u => u.RecordDate).FirstOrDefault()?.UserData).ToList();

         Logger.Info("Found {0} distinct users", freshestUserData.Count);

         return freshestUserData;
      }
   }
}