using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraReporter.JiraApi.Models
{
   [Serializable]
   public class Timesheet
   {
      private readonly List<Worklog> _worklogs = new List<Worklog>();

      public Timesheet(string user)
      {
         User = user;
      }

      public IEnumerable<Worklog> Worklogs => _worklogs;

      public string User { get; private set; }

      public decimal TotalHours
      {
         get
         {
            return Convert.ToDecimal(_worklogs.Sum(x => x.Duration.TotalHours));
         }
      }

      public void AddWorklog(Worklog worklog)
      {
         _worklogs.Add(worklog);
      }

      public decimal GetHoursOnWorklog(Worklog worklog)
      {
         decimal result = Convert.ToDecimal(worklog.Duration.TotalHours);
         return result;
      }

      public decimal GetShareOfEffortOnWorklog(Worklog worklog)
      {
         decimal result = 0;
         if (TotalHours > 0)
         {
            result = GetHoursOnWorklog(worklog) / TotalHours;
         }

         return result;
      }
   }
}