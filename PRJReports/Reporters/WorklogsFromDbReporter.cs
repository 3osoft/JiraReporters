using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain;
using JiraReporterCore.Domain.Users;
using JiraReporterCore.Reporters;
using PRJReports.Database;
using PRJReports.Database.Mappers;

namespace PRJReports.Reporters
{
   public class WorklogsFromDbReporter : BaseReporter<List<Worklog>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly BaseReporter<List<UserData>> _userReporter;
      private readonly JiraToolDbContext _jiraDbContext;
      private readonly DateTime _from;
      private readonly DateTime _till;

      public WorklogsFromDbReporter(BaseReporter<List<UserData>> userReporter, JiraToolDbContext jiraDbContext, DateTime from, DateTime till)
      {
         _userReporter = userReporter;
         _jiraDbContext = jiraDbContext;
         _from = from;
         _till = till;
      }

      protected override List<Worklog> CalculateReportData()
      {
         Logger.Info("Getting worklogs from database in range {0} to {1}", _from, _till);

         var result = new List<Worklog>();
         var userNames = _userReporter.Report().Select(x => x.Login).ToList();

         result = _jiraDbContext.Worklogs
            .Where(x => userNames.Contains(x.User) && x.Date >= _from && x.Date <= _till)
            .Select(WorklogMapper.ToDomain)
            .ToList();

         return result;
      }
   }
}