using System.Collections.Generic;
using JiraReporterCore.Configuration;
using JiraReporterCore.GSheets;

namespace HRReports.GSheets
{
   public abstract class MonthlyHrReportSheet<T> : WritableGoogleSheet<T>
   {
      private readonly string _sheetPrefix;

      protected MonthlyHrReportSheet(GoogleSheetsSettings settings, string sheetPrefix) : base(settings)
      {
         _sheetPrefix = sheetPrefix;
      }

      protected abstract List<IList<object>> GetRowDataToWrite(T dataToWrite);
      protected abstract List<object> GetHeaders();

      public override void Write(T dataToWrite)
      {
         Client.MakeSheetExistAndVisible(GetSheetNameWithPrefix());
         Client.ClearSheet(GetSheetNameWithPrefix());

         List<IList<object>> data = new List<IList<object>> {GetHeaders()};
         data.AddRange(GetRowDataToWrite(dataToWrite));

         Client.WriteToSheet(GetSheetNameWithPrefix(), data);
      }
      protected string GetSheetNameWithPrefix()
      {
         return $"{_sheetPrefix}_{Settings.SheetName}";
      }
   }
}