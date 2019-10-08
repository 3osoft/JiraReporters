using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.JiraApi;
using JiraReporterCore.JiraApi.Models;

namespace JiraReporterCore.Reporters
{
   public class JiraProjectReporter : BaseReporter<List<JiraProject>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly JiraApiClient _apiClient;

      public JiraProjectReporter(JiraApiClient apiClient)
      {
         _apiClient = apiClient;
      }

      protected override List<JiraProject> CalculateReportData()
      {
         Logger.Info("Getting projects");
         var projects = _apiClient.GetJiraProjects().OrderBy(x => x.Id).ToList();
         Logger.Info($"Found {projects.Count} projects");
         return projects;
      }
   }
}