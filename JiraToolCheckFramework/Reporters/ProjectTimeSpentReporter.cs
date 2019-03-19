using System.Collections.Generic;
using System.Linq;
using JiraReporter.Configuration;
using JiraReporter.Reporters;
using JiraToolCheckFramework.GSheets;

namespace JiraToolCheckFramework.Reporters
{
   public class ProjectTimeSpentReporter : BaseReporter<object>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly GoogleSheetsSettings _settings;
      private readonly WorklogsReporter _worklogsReporter;

      public ProjectTimeSpentReporter(GoogleSheetsSettings settings, WorklogsReporter worklogsReporter)
      {
         _settings = settings;
         _worklogsReporter = worklogsReporter;
      }

      protected override object CalculateReportData()
      {
         Logger.Info("Calculating Time spent on projects");

         var timeSpentDictionary = CalculateProjectTimeSpent();

         var projectTimeSpentSheet = new ProjectTimeSpentSheet(_settings);
         projectTimeSpentSheet.WriteBudgetBurned(timeSpentDictionary);

         return null;
      }

      private Dictionary<string, decimal> CalculateProjectTimeSpent()
      {
         Dictionary<string, decimal> timePerProject = new Dictionary<string, decimal>();
         //var worklogsForProjectTimeSpent = GetWorklogs(_userReporter.GetUserNames(), _jiraApiClient, new DateTime(2016, 1, 1), DateTime.Now);
         var worklogsForProjectTimeSpent = _worklogsReporter.Report();

         foreach (var worklogModels in worklogsForProjectTimeSpent.GroupBy(x => x.ProjectKey))
         {
            timePerProject.Add(worklogModels.Key, worklogModels.Sum(x => x.Hours));
         }

         return timePerProject;
      }
   }
}