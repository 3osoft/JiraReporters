using JiraReporter.Domain;

namespace JiraToolCheckFramework.Database.Mappers
{
   public class WorklogMapper
   {
      public static Worklog ToDomain(WorklogModel model)
      {
         return new Worklog
         {
            Date = model.Date,
            Hours = model.Hours,
            User = model.User,
            IssueKey = model.IssueKey,
            Category = model.Category,
            ProjectKey = model.ProjectKey
         };
      }

      public static WorklogModel ToModel(Worklog domain)
      {
         return new WorklogModel
         {
            Date = domain.Date,
            Hours = domain.Hours,
            User = domain.User,
            IssueKey = domain.IssueKey,
            ProjectKey = domain.ProjectKey,
            Category = domain.Category
         };
      }
   }
}