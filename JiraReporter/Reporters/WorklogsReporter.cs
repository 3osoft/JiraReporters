using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporter.Domain;
using JiraReporter.JiraApi;
using JiraReporter.JiraApi.Models;

namespace JiraReporter.Reporters
{
   public class WorklogsReporter : BaseReporter<List<Worklog>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly UserReporter _userReporter;
      private readonly JiraApiClient _jiraApiClient;
      private readonly DateTime _from;
      private readonly DateTime _till;

      public WorklogsReporter(UserReporter userReporter, JiraApiClient jiraApiClient, DateTime from, DateTime till)
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

         Worklogs worklogs = _jiraApiClient.GetWorklogs(_userReporter.GetUserNames(), _from, _till);

         foreach (var user in _userReporter.GetUserNames())
         {
            Timesheet personTimesheet = worklogs.GetTimesheet(user) ?? new Timesheet(user);
            result.AddRange(personTimesheet.Worklogs.Select(x => Worklog.FromWorklog(x, user)));
         }

         return result;
      }
   }
}