﻿using System;
using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Domain.Users;
using JiraReporterCore.Reporters;

namespace HRReports.Reporters
{
   public class OvertimeReporter : BaseReporter<List<Overtime>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly AttendanceReporter _attendanceReporter;
      private readonly UsersActiveInMonthReporter _usersActiveInMonthReporter;
      private readonly MonthWorkHoursReporter _workHoursReporter;
      private readonly int _year;
      private readonly int _month;

      public OvertimeReporter(AttendanceReporter attendanceReporter, 
                              UsersActiveInMonthReporter usersActiveInMonthReporter, 
                              MonthWorkHoursReporter workHoursReporter, 
                              int year, 
                              int month)
      {
         _attendanceReporter = attendanceReporter;
         _usersActiveInMonthReporter = usersActiveInMonthReporter;
         _workHoursReporter = workHoursReporter;
         _year = year;
         _month = month;
      }

      protected override List<Overtime> CalculateReportData()
      {
         var users = _usersActiveInMonthReporter.Report();
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
               LastName = u.LastName,
               CostCenter = u.CostCenter,
               Month = _month,
               Year = _year,
               Absences = gu.Sum(x => x.AbsenceTotal),
               WorkHoursInMonth = CalculateWorkHoursInMonth(u, workHours)
            })
            .ToList();


         return overtimes;
      }

      private int CalculateWorkHoursInMonth(UserData user, int fullWorkHours)
      {
         int result = fullWorkHours;

         bool recalculateForStartDate = user.StartDate.HasValue &&
                                        user.StartDate.Value.Year == _year 
                                        && user.StartDate.Value.Month == _month
                                        && user.StartDate.Value.Day > 1;

         bool recalculateForTerminationDate = user.TerminationDate.HasValue
                                              && user.TerminationDate.Value.Year == _year
                                              && user.TerminationDate.Value.Month == _month;

         if (recalculateForStartDate || recalculateForTerminationDate)
         {
            DateTime startDate = recalculateForStartDate 
               ? user.StartDate.Value 
               : new DateTime(_year, _month, 1);

            DateTime terminationDate = recalculateForTerminationDate
               ? user.TerminationDate.Value
               : new DateTime(_year, _month, 1).AddMonths(1).AddDays(-1);

            MonthWorkHoursReporter userMonthWorkHoursReporter = new MonthWorkHoursReporter(
               _workHoursReporter,
               startDate,
               terminationDate);
            result = userMonthWorkHoursReporter.Report();
         }

         return result;
      }
   }
}