using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain;
using JiraReporterCore.Domain.Users;
using JiraReporterCore.JiraApi;
using JiraReporterCore.JiraApi.Models;

namespace JiraReporterCore.Reporters
{
   public class WorklogsReporter : BaseReporter<List<Worklog>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly BaseReporter<List<UserData>> _userReporter;
      private readonly JiraApiClient _jiraApiClient;
      private readonly DateTime _from;
      private readonly DateTime _till;

      public WorklogsReporter(BaseReporter<List<UserData>> userReporter, JiraApiClient jiraApiClient, DateTime from, DateTime till)
      {
         _userReporter = userReporter;
         _jiraApiClient = jiraApiClient;
         _from = from;
         _till = till;
      }

      protected override List<Worklog> CalculateReportData()
      {
         Logger.Info("Getting worklogs in range {0} to {1}", _from, _till);

         var result = new List<Worklog>();
         var userNames = _userReporter.Report()
            .Select(x => x.Login)
            .ToList();

         Worklogs worklogs = _jiraApiClient.GetWorklogs(userNames, _from, _till);

         foreach (var user in userNames)
         {
            Timesheet personTimesheet = worklogs.GetTimesheet(user) ?? new Timesheet(user);
            result.AddRange(personTimesheet.Worklogs.Select(x => Worklog.FromWorklog(x, user)));
         }

         return result;
      }
   }
}