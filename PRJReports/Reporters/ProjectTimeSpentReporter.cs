using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain;
using JiraReporterCore.Reporters;

namespace PRJReports.Reporters
{
   public class ProjectTimeSpentReporter : BaseReporter<Dictionary<string, decimal>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly BaseReporter<List<Worklog>> _worklogsReporter;

      public ProjectTimeSpentReporter(BaseReporter<List<Worklog>> worklogsReporter)
      {
         _worklogsReporter = worklogsReporter;
      }

      protected override Dictionary<string, decimal> CalculateReportData()
      {
         Logger.Info("Calculating Time spent on projects");
         Dictionary<string, decimal> timePerProject = new Dictionary<string, decimal>();
         var worklogsForProjectTimeSpent = _worklogsReporter.Report();

         foreach (var worklogModels in worklogsForProjectTimeSpent.GroupBy(x => x.ProjectKey))
         {
            timePerProject.Add(worklogModels.Key, worklogModels.Sum(x => x.Hours));
         }

         return timePerProject;  
      }
   }
}