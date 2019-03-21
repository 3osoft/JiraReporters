using System;
using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Domain;

namespace HRReports.HR
{
   public class HrReporting
   {

      public List<Overtime> CalculateOvertimes(IEnumerable<Attendance> attendances, int year, int month, List<UserData> users, int monthWorkHours)
      {
         var timeTrackingUsers = users.Where(x => x.IsTracking.HasValue && x.IsTracking.Value);

         var overtimes = attendances
            .Where(x => x.Date.Month == month && x.Date.Year == year)
            .GroupBy(x => x.User)
            .Join(timeTrackingUsers, gu => gu.Key, u => u.Login, (gu, u) => new Overtime
            {
               Login = gu.Key,
               HoursWorked = gu.Sum(x => x.HoursWorked),
               ContractType = u.ContractType,
               FirstName = u.FirstName,
               LastName = u.FirstName,
               CostCenter = u.CostCenter,
               Month = month,
               Absences = gu.Sum(x => x.AbsenceTotal),
               WorkHoursInMonth = monthWorkHours
            })
            .ToList();

         foreach (var overtime in overtimes)
         {
            overtime.CalculateOvertime();
         }

         return overtimes;
      }
   }
}