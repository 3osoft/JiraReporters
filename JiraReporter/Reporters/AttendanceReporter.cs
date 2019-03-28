using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain;
using JiraReporterCore.Domain.Users;
using JiraReporterCore.JiraApi.Models;
using JiraReporterCore.Utils;

namespace JiraReporterCore.Reporters
{
   public class AttendanceReporter : BaseReporter<List<Attendance>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly BaseReporter<List<UserData>> _userReporter;
      private readonly AbsenceReporter _absenceReporter;
      private readonly BaseReporter<List<Worklog>> _worklogsReporter;
      private readonly DateTime _from;
      private readonly DateTime _till;

      public AttendanceReporter(BaseReporter<List<UserData>> userReporter, AbsenceReporter absenceReporter, BaseReporter<List<Worklog>> worklogsReporter, DateTime from, DateTime till)
      {
         _userReporter = userReporter;
         _absenceReporter = absenceReporter;
         _worklogsReporter = worklogsReporter;
         _from = from;
         _till = till;
      }

      protected override List<Attendance> CalculateReportData()
      {
         Logger.Info("Calculating attendance");
         var userNames = _userReporter.Report().Select(x => x.Login).ToList();
         var attendance = GetAttendances(userNames, _absenceReporter.Report(), _worklogsReporter.Report(), _from, _till);
         Logger.Info("Writing to time grid sheet");

         return attendance;
      }

      private List<Attendance> GetAttendances(List<string> users, List<Absence> absences, List<Worklog> worklogs, DateTime from, DateTime to)
      {
         List<Attendance> result = new List<Attendance>();

         foreach (DateTime day in DateTimeUtils.EachDay(from, to))
         {
            foreach (var user in users)
            {
               var userDateAbsences = absences.Where(x => x.Date.Equals(day) && x.UserName.Equals(user)).ToList();
               var userDateWorklogs = worklogs.Where(x => x.Date.Equals(day) && x.User.Equals(user));

               var newAttendanceModel = new Attendance
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

               result.Add(newAttendanceModel);
            }
         }

         return result;
      }
   }
}