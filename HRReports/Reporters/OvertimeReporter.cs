﻿using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Reporters;

namespace HRReports.Reporters
{
   public class OvertimeReporter : BaseReporter<List<Overtime>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly AttendanceReporter _attendanceReporter;
      private readonly CurrentUsersReporter _currentUserReporter;
      private readonly MonthWorkHoursReporter _workHoursReporter;
      private readonly int _year;
      private readonly int _month;

      public OvertimeReporter(AttendanceReporter attendanceReporter, CurrentUsersReporter currentUserReporter, MonthWorkHoursReporter workHoursReporter, int year, int month)
      {
         _attendanceReporter = attendanceReporter;
         _currentUserReporter = currentUserReporter;
         _workHoursReporter = workHoursReporter;
         _year = year;
         _month = month;
      }

      protected override List<Overtime> CalculateReportData()
      {
         var users = _currentUserReporter.Report();
         var attendances = _attendanceReporter.Report();

         var timeTrackingUsers = users.Where(x => x.IsTracking.HasValue && x.IsTracking.Value);
         var workHours = _workHoursReporter.Report();

         Logger.Info("Calculating overtimes for {0} users", users.Count);

         var overtimes = attendances
            .Where(x => x.Date.Month == _month && x.Date.Year == _year)
            .GroupBy(x => x.User)
            .Join(timeTrackingUsers, gu => gu.Key, u => u.Login, (gu, u) => new Overtime
            {
               Login = gu.Key,
               HoursWorked = gu.Sum(x => x.HoursWorked),
               ContractType = u.ContractType,
               FirstName = u.FirstName,
               LastName = u.FirstName,
               CostCenter = u.CostCenter,
               Month = _month,
               Year = _year,
               Absences = gu.Sum(x => x.AbsenceTotal),
               WorkHoursInMonth = workHours
            })
            .ToList();


         return overtimes;
      }
   }
}