using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Database;

namespace JiraToolCheckFramework.GSheets
{
   public class AttendanceGridSheet : GoogleSheet
   {
      private const string UsersColumnHeader = "Users";

      public AttendanceGridSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public void WriteAttendance(List<AttendanceModel> attendances)
      {
         _client.ClearSheet(_settings.SheetName);

         var availableDates = attendances.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();
         var groupedAttendances = attendances.OrderBy(x => x.Date).GroupBy(x => x.User);
         var resultData = new List<IList<object>>();
         List<object> headerFirstRow = new List<object> { UsersColumnHeader };
         headerFirstRow.AddRange(availableDates.Select(x => x.ToString("MM/dd/yyyy")));

         List<object> headerSecondRow = new List<object> {""};
         headerSecondRow.AddRange(availableDates.Select(x => x.DayOfWeek.ToString()));

         resultData.Add(headerFirstRow);
         resultData.Add(headerSecondRow);

         foreach (var groupedAttendance in groupedAttendances)
         {
            var newRow = new List<object> {groupedAttendance.Key};
            newRow.AddRange(groupedAttendance.OrderBy(x => x.Date).Select(x => $"={x.HoursWorked.ToString(CultureInfo.GetCultureInfo("en-US"))}+{x.AbsenceTotal.ToString(CultureInfo.GetCultureInfo("en-US"))}"));

            resultData.Add(newRow);
         }

         _client.WriteToSheet(_settings.SheetName, resultData);
      }
   }
}