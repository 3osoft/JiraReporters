using System.Collections.Generic;
using System.Linq;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Database;
using JiraToolCheckFramework.GSheets.FinanceSimulator.Utilities;

namespace JiraToolCheckFramework.GSheets
{
   public class TimeGridSheet
   {
      private readonly GSheet _timeGridGSheet;
      private readonly GoogleSheetsSettings _settings;

      public TimeGridSheet(GoogleSheetsSettings settings)
      {
         _settings = settings;
         _timeGridGSheet = new GSheet(_settings.GoogleSheetId);
      }

      public void WriteAttendance(List<AttendanceModel> attendances)
      {
         _timeGridGSheet.ClearSheet("attendance");
         //_timeGridGSheet.DeleteAllRowsAndColumns("attendance");

         var availableDates = attendances.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();

         var groupedAttendances = attendances.OrderBy(x => x.Date).GroupBy(x => x.User);

         var resultData = new List<IList<object>>();


         List<object> headerFirstRow = new List<object> {"Users"};
         headerFirstRow.AddRange(availableDates.Select(x => x.ToShortDateString()));

         List<object> headerSecondRow = new List<object> {""};
         headerSecondRow.AddRange(availableDates.Select(x => x.DayOfWeek.ToString()));

         resultData.Add(headerFirstRow);
         resultData.Add(headerSecondRow);

         foreach (var groupedAttendance in groupedAttendances)
         {
            var newRow = new List<object> {groupedAttendance.Key};
            newRow.AddRange(groupedAttendance.OrderBy(x => x.Date).Select(x => $"={x.HoursWorked}+{x.AbsenceTotal}"));

            resultData.Add(newRow);
         }


         _timeGridGSheet.WriteToSheet("attendance", resultData);
      }
   }
}