using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRJReports.Database
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
   }
}