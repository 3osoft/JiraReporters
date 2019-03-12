using System;

namespace JiraToolCheckFramework.JiraApi
{
   [Serializable]
   public class Absence
   {
      public int Id { get; set; }
      public string IssueKey { get; set; }
      public string Name { get; set; }
      public string Status { get; set; }
      public DateTime CreatedDate { get; set; }
      public DateTime StartDate { get; set; }
      public DateTime EndDate { get; set; }
      public decimal Duration { get; set; }
      public AbsenceDayHourEnum DurationType { get; set; }
      public string AbsenceCategory { get; set; }
   }
}
