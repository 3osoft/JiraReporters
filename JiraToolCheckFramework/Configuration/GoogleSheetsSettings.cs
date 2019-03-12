namespace JiraToolCheckFramework.Configuration
{
   public class GoogleSheetsSettings
   {
      public string GoogleSheetId { get; set; }
      public string UserSheetName { get; set; }
      public int UserSheetRowsToSkip { get; set; }
      public int UserSheetLoginColumnIndex { get; set; }
   }
}
