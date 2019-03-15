using System;
using JiraToolCheckFramework.JiraApi;

namespace JiraToolCheckFramework.Configuration
{
   public class Config
   {
      public JiraSettings JiraSettings { get; set; }
      public GmailSettings SinnerNotifierGmailSettings { get; set; }
      public GoogleSheetsSettings UsersSheetSettings { get; set; }
      public GoogleSheetsSettings AttendanceGridSheetSettings { get; set; }
      public GoogleSheetsSettings ProjectTimeSpentSheetSettings { get; set; }
      public GoogleSheetsSettings RunLogSheetSettings { get; set; }
      public GoogleSheetsSettings SinnersSheetSettings { get; set; }
      public string PublicHolidayApiKey { get; set; }
      public DateTime? DateFrom { get; set; }
      public DateTime? DateTo { get; set; }
   }
}