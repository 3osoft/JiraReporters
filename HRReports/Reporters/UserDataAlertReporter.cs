using System;
using System.Collections.Generic;
using HRReports.Alerts;
using JiraReporterCore.Reporters;
using JiraReporterCore.Reporters.Users;

namespace HRReports.Reporters
{
   public class UserDataAlertReporter : BaseReporter<List<BaseAlert>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly CurrentUsersReporter _currentUsersReporter;
      private readonly DateTime _currentDate;

      public UserDataAlertReporter(CurrentUsersReporter currentUsersReporter, DateTime currentDate)
      {
         _currentUsersReporter = currentUsersReporter;
         _currentDate = currentDate;
      }

      protected override List<BaseAlert> CalculateReportData()
      {
         Logger.Info("Resolving user data alerts");
         var result = new List<BaseAlert>();
         var users = _currentUsersReporter.Report();

         result.AddRange(EndOfProbationAlert.FromUserData(users, _currentDate));
         result.AddRange(EndOfContractAlert.FromUserData(users, _currentDate));
         result.AddRange(EmploymentAnniversaryAlert.FromUserData(users, _currentDate));
         result.AddRange(BirthdayAlert.FromUserData(users, _currentDate));
         Logger.Info("Found {0} user data alerts", result.Count);

         return result;
      }
   }
}