using System;
using System.Collections.Generic;
using System.Linq;
using JiraToolCheckFramework.Database;
using JiraToolCheckFramework.JiraApi;
using JiraToolCheckFramework.JiraApi.Models;

namespace JiraToolCheckFramework.Reporters
{
   public class WorklogsReporter : BaseReporter<List<WorklogModel>>
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

      protected override List<WorklogModel> CalculateReportData()
      {
         Logger.Info("Getting worklogs in range {0} to {1}", _from, _till);

         var result = new List<WorklogModel>();

         Worklogs worklogs = _jiraApiClient.GetWorklogs(_userReporter.GetUserNames(), _from, _till);

         foreach (var user in _userReporter.GetUserNames())
         {
            Timesheet personTimesheet = worklogs.GetTimesheet(user) ?? new Timesheet(user);
            result.AddRange(personTimesheet.Worklogs.Select(x => WorklogModel.FromWorklog(x, user)));
         }

         return result;
      }
   }
}