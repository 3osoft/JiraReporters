using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraReporterCore.JiraApi.Models
{
   [Serializable]
   public class JiraWorklog
   {
      public string IssueKey { get; set; }
      public IEnumerable<string> Labels { get; set; }
      public DateTime Started { get; set; }
      public TimeSpan Duration { get; set; }

      public string Category
      {
         get
         {
            string result = "Unknown";

            if (Labels != null && Labels.Any())
            {
               result = Labels.Count() > 1 ? "Multiple" : Labels.Single();
            }

            return result;
         }
      }

      public string ProjectKey
      {
         get
         {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(IssueKey))
            {
               result = IssueKey.Split('-')[0];
            }

            return result;
         }
      }
   }
}