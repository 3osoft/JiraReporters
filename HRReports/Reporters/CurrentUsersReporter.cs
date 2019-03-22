using System;
using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Reporters;

namespace HRReports.Reporters
{
   public class CurrentUsersReporter : BaseReporter<List<UserData>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly RawUserDataReporter _rawUserDataReporter;

      public CurrentUsersReporter(RawUserDataReporter rawUserDataReporter)
      {
         _rawUserDataReporter = rawUserDataReporter;
      }

      protected override List<UserData> CalculateReportData()
      {
         Logger.Info("Getting current users");
         var groupedUserData = _rawUserDataReporter.Report().GroupBy(u => new
         {
            u.UserData.Login
         });

         var freshestUserData = groupedUserData.Select(x => x.OrderByDescending(u => u.RecordDate).FirstOrDefault()?.UserData).ToList();

         var activeUsers = freshestUserData
            .Where(x => !x.TerminationDate.HasValue || x.TerminationDate.Value.Date > DateTime.Now.Date).ToList();

         Logger.Info("Found {0} distinct users, {1} active ones", freshestUserData.Count, activeUsers.Count);

         return activeUsers;
      }
   }
}