using JiraReporter.Configuration;

namespace JiraReporter.GSheets
{
   public abstract class GoogleSheet
   {
      protected readonly GoogleSheetsSettings Settings;
      protected readonly GoogleSheetClient Client;

      protected GoogleSheet(GoogleSheetsSettings settings)
      {
         Settings = settings;
         Client = new GoogleSheetClient(Settings.GoogleSheetId);
      }
   }
}