using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Reporters;

namespace PRJReports.Reporters
{
   public class ProjectTimeSpentReporter : BaseReporter<Dictionary<string, decimal>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly WorklogsReporter _worklogsReporter;

      public ProjectTimeSpentReporter(WorklogsReporter worklogsReporter)
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