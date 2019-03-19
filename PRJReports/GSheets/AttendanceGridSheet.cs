using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JiraReporterCore.Configuration;
using JiraReporterCore.Domain;
using JiraReporterCore.GSheets;

namespace PRJReports.GSheets
{
   public class AttendanceGridSheet : WritableGoogleSheet<List<Attendance>>
   {
      private const string UsersColumnHeader = "Users";

      public AttendanceGridSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public override void Write(List<Attendance> dataToWrite)
      {
         Client.ClearSheet(Settings.SheetName);

         var availableDates = dataToWrite.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();
         var groupedAttendances = dataToWrite.OrderBy(x => x.Date).GroupBy(x => x.User);
         var resultData = new List<IList<object>>();
         List<object> headerFirstRow = new List<object> { UsersColumnHeader };
         headerFirstRow.AddRange(availableDates.Select(x => x.ToString("MM/dd/yyyy")));

         List<object> headerSecondRow = new List<object> { "" };
         headerSecondRow.AddRange(availableDates.Select(x => x.DayOfWeek.ToString()));

         resultData.Add(headerFirstRow);
         resultData.Add(headerSecondRow);

         foreach (var groupedAttendance in groupedAttendances)
         {
            var newRow = new List<object> { groupedAttendance.Key };
            newRow.AddRange(groupedAttendance.OrderBy(x => x.Date).Select(x => $"={x.HoursWorked.ToString(CultureInfo.GetCultureInfo("en-US"))}+{x.AbsenceTotal.ToString(CultureInfo.GetCultureInfo("en-US"))}"));

            resultData.Add(newRow);
         }

         Client.WriteToSheet(Settings.SheetName, resultData);
      }
   }
}