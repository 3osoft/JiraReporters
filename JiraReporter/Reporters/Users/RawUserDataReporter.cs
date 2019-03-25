using System.Collections.Generic;
using JiraReporterCore.Domain.Users;
using JiraReporterCore.GSheets;

namespace JiraReporterCore.Reporters.Users
{
   public class RawUserDataReporter : BaseReporter<List<RawUserData>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly RawUserDataSheet _rawUserDataSheet;

      public RawUserDataReporter(RawUserDataSheet rawUserDataSheet)
      {
         _rawUserDataSheet = rawUserDataSheet;
      }

      protected override List<RawUserData> CalculateReportData()
      {
         Logger.Info("Getting raw user data");
         var rawUserData = _rawUserDataSheet.GetRawUserData();
         Logger.Info("Found {0} user data records", rawUserData.Count);
         return rawUserData;
      }
   }
}