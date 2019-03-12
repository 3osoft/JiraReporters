using System;
using JiraToolCheckFramework.JiraApi;

namespace JiraToolCheckFramework.Configuration
{
   public class Config
   {
      public JiraSettings JiraSettings { get; set; }
      public GoogleSheetsSettings GoogleSheetsSettings { get; set; }
      public string PublicHolidayApiKey { get; set; }
      public DateTime DateFrom { get; set; }
   }
}