using JiraToolCheckFramework.Configuration;

namespace JiraToolCheckFramework.GSheets
{
   public abstract class GoogleSheet
   {
      protected readonly GoogleSheetsSettings _settings;
      protected readonly GoogleSheetClient _client;

      protected GoogleSheet(GoogleSheetsSettings settings)
      {
         _settings = settings;
         _client = new GoogleSheetClient(_settings.GoogleSheetId);
      }
   }
}