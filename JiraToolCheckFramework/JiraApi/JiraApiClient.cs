using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JiraToolCheckFramework.JiraApi.Models;
using RestSharp;
using RestSharp.Authenticators;

namespace JiraToolCheckFramework.JiraApi
{

   public class JiraApiClient
   {
      private static readonly NewtonsoftJsonSerializer _newtonsoftJsonSerializer = new NewtonsoftJsonSerializer();
      private readonly ConcurrentDictionary<string, dynamic> _issuesCache = new ConcurrentDictionary<string, dynamic>();

      private readonly RestClient _restClient;

      public JiraApiClient(JiraSettings jiraSettings)
      {
         _restClient = new RestClient(new Uri(jiraSettings.BaseUrl, "rest/api/2"))
         {
            Authenticator = new HttpBasicAuthenticator(jiraSettings.Login, jiraSettings.Password)
         };
         _restClient.AddHandler("application/json", new DynamicJsonDeserializer());
      }

      public virtual Worklogs GetWorklogs(IEnumerable<string> users, DateTime from, DateTime till)
      {
         Worklogs results = new Worklogs();

         ConcurrentBag<dynamic> issuesJsons = new ConcurrentBag<dynamic>();
         dynamic firstIssuesJson = GetIssuesWithWorklogs(users, from, till, 0);
         PagingJob issuesPagingJob = new PagingJob(firstIssuesJson.issues.Count, firstIssuesJson.total.Value);
         issuesJsons.Add(firstIssuesJson);

         if (issuesPagingJob.IsPagingNecessary)
         {
            Parallel.ForEach(issuesPagingJob.GetPageStarts(), pageStart =>
            {
               issuesJsons.Add(GetIssuesWithWorklogs(users, from, till, pageStart));
            });
         }

         ConcurrentBag<dynamic> worklogsJsons = new ConcurrentBag<dynamic>();
         ConcurrentBag<WorklogsPagingJob> worklogsPagingJobs = new ConcurrentBag<WorklogsPagingJob>();
         foreach (dynamic issuesJson in issuesJsons)
         {
            Parallel.ForEach(issuesJson.issues, (Action<dynamic>)(issueJson =>
            {
               if (issueJson.fields != null && issueJson.fields.worklog != null)
               {
                  List<string> labels = new List<string>();
                  if (issueJson.fields.parent != null)
                  {
                     dynamic issueParent = GetIssue((string)issueJson.fields.parent.key, "labels");
                     foreach (dynamic label in issueParent.fields.labels)
                     {
                        labels.Add((string)label);
                     }
                  }
                  else if (issueJson.fields.labels != null)
                  {
                     foreach (dynamic label in issueJson.fields.labels)
                     {
                        labels.Add((string)label);
                     }
                  }

                  dynamic firstWorklogsJson = issueJson.fields.worklog;
                  WorklogsPagingJob worklogsPagingJob = new WorklogsPagingJob(issueJson.key.Value, labels, firstWorklogsJson.worklogs.Count, firstWorklogsJson.total.Value);
                  if (worklogsPagingJob.IsPagingNecessary)
                  {
                     worklogsPagingJobs.Add(worklogsPagingJob);
                  }
                  else
                  {
                     firstWorklogsJson.issueKey = issueJson.key;
                     firstWorklogsJson.labels = String.Join("||", labels);
                     worklogsJsons.Add(firstWorklogsJson);
                  }
               }
               else
               {
                  worklogsPagingJobs.Add(new WorklogsPagingJob(issueJson.key.Value, Enumerable.Empty<string>(), 0, 1000));
               }
            }));
         }

         Parallel.ForEach(worklogsPagingJobs, x =>
         {
            dynamic worklogsJson = GetWorklogsForIssue(x.IssueKey, 0);
            worklogsJson.issueKey = x.IssueKey;
            worklogsJson.labels = String.Join("||", x.Labels);
            worklogsJsons.Add(worklogsJson);
         });

         foreach (dynamic worklogJsons in worklogsJsons)
         {
            string issueKey = worklogJsons.issueKey;
            string labels = worklogJsons.labels ?? String.Empty;
            foreach (dynamic worklogJson in worklogJsons.worklogs)
            {
               DateTime startTime = worklogJson.started.Value;
               string workLoggedByUserName = worklogJson.author.name.Value;
               string workLoggedByUser = users.SingleOrDefault(x => x == workLoggedByUserName);
               if (startTime >= from && startTime <= till && workLoggedByUser != null)
               {
                  Worklog worklog = new Worklog
                  {
                     IssueKey = issueKey,
                     Labels = labels.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries),
                     Started = startTime,
                     Duration = TimeSpan.FromSeconds(Convert.ToDouble(worklogJson.timeSpentSeconds.Value))
                  };
                  results.AddWorklogForUser(workLoggedByUser, worklog);
               }
            }
         }

         return results;
      }

      private dynamic GetIssuesWithWorklogs(IEnumerable<string> users, DateTime from, DateTime till, long startAt)
      {
         string jql = String.Format(
            "worklogAuthor in ({0}) AND worklogDate >= \"{1:yyyy-MM-dd}\" AND worklogDate <= \"{2:yyyy-MM-dd}\"", String.Join(", ", users), from, till);

         return GetIssuesPage(jql, startAt, "worklog,parent,labels", null);
      }

      private dynamic GetWorklogsForIssue(string issueKey, long startAt)
      {
         RestRequest restRequest = CreateRequest("issue/" + issueKey + "/worklog", Method.GET);
         restRequest.AddQueryParameter("startAt", startAt.ToString());

         IRestResponse<dynamic> restResponse = _restClient.Execute<dynamic>(restRequest);
         restResponse.EnsureSuccessStatusCode();

         return restResponse.Data;
      }

      public dynamic GetIssue(string key, string fields)
      {
         dynamic result = _issuesCache.GetOrAdd(key, x =>
          {
             dynamic output = null;

             dynamic issuesPage = GetIssuesPage("key = " + x, 0, fields, null);
             foreach (dynamic issue in issuesPage.issues)
             {
                if (output == null)
                {
                   output = issue;
                   break;
                }
             }

             return output;
          });
         return result;
      }

      private dynamic GetIssuesPage(string jql, long startAt, string fields, string expand)
      {
         RestRequest restRequest = CreateRequest("search", Method.GET);
         restRequest.AddQueryParameter("jql", jql);
         restRequest.AddQueryParameter("startAt", startAt.ToString());
         if (!String.IsNullOrEmpty(fields))
         {
            restRequest.AddQueryParameter("fields", fields);
         }
         if (!String.IsNullOrEmpty(expand))
         {
            restRequest.AddQueryParameter("expand", expand);
         }

         IRestResponse<dynamic> restResponse = _restClient.Execute<dynamic>(restRequest);
         restResponse.EnsureSuccessStatusCode();

         return restResponse.Data;
      }

      public IEnumerable<Absence> GetAbsences(Dictionary<string, string> userNameInitialsDictionary)
      {
         var result = new List<Absence>();
         ConcurrentBag<dynamic> issuesJsons = new ConcurrentBag<dynamic>();
         
         var items = GetAbsencePage(0);

         PagingJob issuesPagingJob = new PagingJob(items.issues.Count, items.total.Value);
         issuesJsons.Add(items);

         if (issuesPagingJob.IsPagingNecessary)
         {
            Parallel.ForEach(issuesPagingJob.GetPageStarts(), pageStart =>
            {
               issuesJsons.Add(GetAbsencePage(pageStart));
            });
         }

         foreach (var item in issuesJsons)
         {
            foreach (var issue in item.issues)
            {
               result.Add(AbsenceMapper.MapAbsence(issue, userNameInitialsDictionary));
            }
         }

         return result;
      }

      public IEnumerable<IEnumerable<object>> GetAbsenceMatrix(Dictionary<string, string> userNameInitialsDictionary)
      {
         var result = new List<List<object>>();
         var absences = GetAbsences(userNameInitialsDictionary);

         foreach (var item in absences)
         {
            List<object> absence = new List<object>();
            foreach (PropertyInfo propertyInfo in item.GetType().GetProperties())
            {
               absence.Add(propertyInfo.GetValue(item, null));
            }
            result.Add(absence);
         }

         return result;
      }

      private dynamic GetAbsencePage(long startAt)
      {
         RestRequest restRequest = CreateRequest("search", Method.GET);
         restRequest.AddQueryParameter("jql", "project = ABS");
         restRequest.AddQueryParameter("fields", $"creator,created,summary,status," +
                                                 $"{CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsEndDate]}," +
                                                 $"{CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsStartDate]}," +
                                                 $"{CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsActualNumberOfDaysHours]}," +
                                                 $"{CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsDaysHours]}," +
                                                 $"{CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsAbsenceCategory]}");

         restRequest.AddQueryParameter("startAt", startAt.ToString());

         IRestResponse<dynamic> restResponse = _restClient.Execute<dynamic>(restRequest);
         restResponse.EnsureSuccessStatusCode();

         return restResponse.Data;
      }

      private static RestRequest CreateRequest(string resource, Method method)
      {
         return new RestRequest(resource, method)
         {
            JsonSerializer = _newtonsoftJsonSerializer
         };
      }
   }
}