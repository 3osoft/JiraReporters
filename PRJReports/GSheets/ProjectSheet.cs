using System.Collections.Generic;
using JiraReporterCore.Configuration;
using JiraReporterCore.GSheets;
using JiraReporterCore.JiraApi.Models;

namespace PRJReports.GSheets
{
   public class ProjectSheet : WritableGoogleSheet<List<JiraProject>>
   {
      private readonly string _writeRange;

      public ProjectSheet(GoogleSheetSettingsWithRange settings) : base(settings)
      {
         _writeRange = settings.Range;
      }

      public override void Write(List<JiraProject> projects)
      {
         List<IList<object>> dataToWrite = new List<IList<object>> { new List<object> { "Key", "Name", "Category" } };

         foreach (var project in projects)
         {
            dataToWrite.Add(new List<object>
            {
               project.Key,
               project.Name,
               project.Category
            });
         }

         Client.ClearSheetAtRange(Settings.SheetName, _writeRange);

         Client.WriteToSheetAtRange(Settings.SheetName, dataToWrite, _writeRange);
      }
   }
}