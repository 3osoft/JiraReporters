using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain.Users;

namespace JiraReporterCore.Reporters.Users
{
   public class CurrentUsersReporter : BaseReporter<List<UserData>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly FreshestUserDataReporter _freshestUserDataReporter;

      public CurrentUsersReporter(FreshestUserDataReporter freshestUserDataReporter)
      {
         _freshestUserDataReporter = freshestUserDataReporter;
      }

      protected override List<UserData> CalculateReportData()
      {
         Logger.Info("Getting current active users");

         var freshestUserData = _freshestUserDataReporter.Report();

         var activeUsers = freshestUserData
            .Where(x => !x.TerminationDate.HasValue || x.TerminationDate.Value.Date > DateTime.Now.Date).ToList();

         Logger.Info("Found {0} distinct users, {1} active ones", freshestUserData.Count, activeUsers.Count);

         return activeUsers;
      }
   }
}