using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.JiraApi;
using JiraReporterCore.JiraApi.Models;

namespace JiraReporterCore.Reporters
{
   public class JiraAbsenceReporter : BaseReporter<List<JiraAbsence>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private static readonly string[] AbsenceStatusesToBeIgnored =
      {
         "Canceled",
         "Rejected"
      };

      private readonly UserReporter _userReporter;
      private readonly JiraApiClient _jiraApiClient;

      public JiraAbsenceReporter(UserReporter userReporter, JiraApiClient jiraApiClient)
      {
         _userReporter = userReporter;
         _jiraApiClient = jiraApiClient;
      }

      protected override List<JiraAbsence> CalculateReportData()
      {
         Logger.Info("Getting jira absences");
         var initialsDictionary = _userReporter.Report().ToDictionary(x => x.Initials, x => x.UserName);
         var allStatusAbsences = _jiraApiClient.GetAbsences(initialsDictionary).ToList();
         var absences = allStatusAbsences.Where(x => !AbsenceStatusesToBeIgnored.Contains(x.Status)).ToList();
         Logger.Info("Found {0} absences in all status, {1} in usable status", allStatusAbsences.Count, absences.Count);

         return absences;
      }
   }
}