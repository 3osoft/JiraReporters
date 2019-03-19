using JiraReporterCore.Configuration;
using JiraReporterCore.Reporters.Writer;

namespace JiraReporterCore.GSheets
{
   public abstract class WritableGoogleSheet<T> : GoogleSheet, IWritable<T>
   {
      protected WritableGoogleSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public abstract void Write(T dataToWrite);
   }
}