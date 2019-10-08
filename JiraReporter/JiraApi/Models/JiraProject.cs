using System;

namespace JiraReporterCore.JiraApi.Models
{
   [Serializable]
   public class JiraProject
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Key { get; set; }
      public string Category { get; set; }
   }
}