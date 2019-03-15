using System;
using System.Collections.Generic;
using System.Linq;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Database;
using JiraToolCheckFramework.GSheets;
using JiraToolCheckFramework.JiraApi;
using JiraToolCheckFramework.Utils;

namespace JiraToolCheckFramework.Reporters
{
   public class AttendanceReporter : BaseReporter<List<AttendanceModel>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly UserReporter _userReporter;
      private readonly AbsenceReporter _absenceReporter;
      private readonly WorklogsReporter _worklogsReporter;
      private readonly DateTime _from;
      private readonly DateTime _till;
      private readonly GoogleSheetsSettings _settings;

      public AttendanceReporter(UserReporter userReporter, AbsenceReporter absenceReporter, WorklogsReporter worklogsReporter, DateTime from, DateTime till, GoogleSheetsSettings settings)
      {
         _userReporter = userReporter;
         _absenceReporter = absenceReporter;
         _worklogsReporter = worklogsReporter;
         _from = from;
         _till = till;
         _settings = settings;
      }

      protected override List<AttendanceModel> CalculateReportData()
      {
         Logger.Info("Calculating attendance");

         var attendance = GetAttendances(_userReporter.GetUserNames(), _absenceReporter.Report(), _worklogsReporter.Report(), _from, _till);
         Logger.Info("Writing to time grid sheet");
         var timeGridSheet = new AttendanceGridSheet(_settings);
         timeGridSheet.WriteAttendance(attendance);

         return attendance;
      }

      private List<AttendanceModel> GetAttendances(List<string> users, List<AbsenceModel> absences, List<WorklogModel> worklogs, DateTime from, DateTime to)
      {
         List<AttendanceModel> result = new List<AttendanceModel>();

         foreach (DateTime day in DateTimeUtils.EachDay(from, to))
         {
            foreach (var user in users)
            {
               var userDateAbsences = absences.Where(x => x.Date.Equals(day) && x.UserName.Equals(user)).ToList();
               var userDateWorklogs = worklogs.Where(x => x.Date.Equals(day) && x.User.Equals(user));

               var newAttendanceModel = new AttendanceModel
               {
                  Date = day,
                  User = user,
                  HoursWorked = userDateWorklogs.Sum(x => x.Hours),
                  AbsenceDoctor = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.Doctor).Sum(x => x.Hours),
                  AbsenceDoctorFamily = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.DoctorFamily).Sum(x => x.Hours),
                  AbsenceIllness = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.Illness).Sum(x => x.Hours),
                  AbsencePersonalLeave = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.PersonalLeave).Sum(x => x.Hours),
                  AbsenceVacation = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.Vacation).Sum(x => x.Hours),
               };

               newAttendanceModel.CalculateTotals();
               result.Add(newAttendanceModel);
            }
         }

         return result;
      }
   }
}