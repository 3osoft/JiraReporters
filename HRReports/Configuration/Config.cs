using JiraReporterCore.Configuration;
using JiraReporterCore.JiraApi;

namespace HRReports.Configuration
{

   public class Config
   {
      public JiraSettings JiraSettings { get; set; }
      public GoogleSheetsSettings RawUsersSheetSettings { get; set; }
      public GoogleSheetsSettings CurrentUsersSheetSettings { get; set; }
      public GoogleSheetsSettings OvertimeSheetSettings { get; set; }
      public GoogleSheetsSettings SalaryDataSheetSettings { get; set; }
      public string PublicHolidayApiKey { get; set; }
   }
}