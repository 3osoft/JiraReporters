using System.Collections.Generic;

namespace JiraReporterCore.JiraApi
{
   public class WorklogsPagingJob : PagingJob
   {
      public string IssueKey { get; }
      public IEnumerable<string> Labels { get; }

      public WorklogsPagingJob(string issueKey, IEnumerable<string> labels, long actualResults, long totalResults) : base(actualResults, totalResults)
      {
         IssueKey = issueKey;
         Labels = labels;
      }
   }
}