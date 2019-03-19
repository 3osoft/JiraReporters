using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraReporterCore.JiraApi.Models
{
   [Serializable]
   public class Timesheet
   {
      private readonly List<JiraWorklog> _worklogs = new List<JiraWorklog>();

      public Timesheet(string user)
      {
         User = user;
      }

      public IEnumerable<JiraWorklog> Worklogs => _worklogs;

      public string User { get; private set; }

      public decimal TotalHours
      {
         get
         {
            return Convert.ToDecimal(_worklogs.Sum(x => x.Duration.TotalHours));
         }
      }

      public void AddWorklog(JiraWorklog jiraWorklog)
      {
         _worklogs.Add(jiraWorklog);
      }

      public decimal GetHoursOnWorklog(JiraWorklog jiraWorklog)
      {
         decimal result = Convert.ToDecimal(jiraWorklog.Duration.TotalHours);
         return result;
      }

      public decimal GetShareOfEffortOnWorklog(JiraWorklog jiraWorklog)
      {
         decimal result = 0;
         if (TotalHours > 0)
         {
            result = GetHoursOnWorklog(jiraWorklog) / TotalHours;
         }

         return result;
      }
   }
}