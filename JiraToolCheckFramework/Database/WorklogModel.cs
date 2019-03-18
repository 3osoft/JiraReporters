using System;
using System.ComponentModel.DataAnnotations.Schema;
using JiraToolCheckFramework.JiraApi.Models;

namespace JiraToolCheckFramework.Database
{
   public class WorklogModel
   {
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int Id { get; set; }

      public string User { get; set; }
      public string Category { get; set; }
      public decimal Hours { get; set; }
      public string IssueKey { get; set; }
      public string ProjectKey { get; set; }
      public DateTime Date { get; set; }

      public static WorklogModel FromWorklog(Worklog worklog, string user)
      {
         return new WorklogModel
         {
            User = user,
            Category = worklog.Category,
            Date = worklog.Started.Date,
            Hours = Convert.ToDecimal(worklog.Duration.TotalHours),
            IssueKey = worklog.IssueKey,
            ProjectKey = worklog.ProjectKey
         };
      }
   }
}