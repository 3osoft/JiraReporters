using System;
using JiraReporterCore.JiraApi.Models;

namespace JiraReporterCore.Domain
{
   public class Worklog
   {
      public string User { get; set; }
      public string Category { get; set; }
      public decimal Hours { get; set; }
      public string IssueKey { get; set; }
      public string ProjectKey { get; set; }
      public DateTime Date { get; set; }

      public static Worklog FromWorklog(JiraWorklog jiraWorklog, string user)
      {
         return new Worklog
         {
            User = user,
            Category = jiraWorklog.Category,
            Date = jiraWorklog.Started.Date,
            Hours = Convert.ToDecimal(jiraWorklog.Duration.TotalHours),
            IssueKey = jiraWorklog.IssueKey,
            ProjectKey = jiraWorklog.ProjectKey
         };
      }
   }
}