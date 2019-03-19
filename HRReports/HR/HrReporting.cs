using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain;

namespace HRReports.HR
{
   public class HrReporting
   {
      public List<UserData> GetCurrentUsers(List<RawUserData> userData)
      {
         var groupedUserData = userData.GroupBy(u => new
         {
            u.UserData.Login
         });

         var freshestUserData = groupedUserData.Select(x =>
         {
            var orderedRecords = x.OrderByDescending(u => u.RecordDate).ToList();

            return new UserData
            {
               StartDate = orderedRecords.FirstOrDefault(u => u.UserData.StartDate != null)?.UserData.StartDate,
               IsTracking = orderedRecords.FirstOrDefault(u => u.UserData.IsTracking != null)?.UserData.IsTracking,
               Initials = orderedRecords.FirstOrDefault(u => u.UserData.Initials != null)?.UserData.Initials,
               Login = orderedRecords.FirstOrDefault(u => u.UserData.Login != null)?.UserData.Login,
               Benefit = orderedRecords.FirstOrDefault(u => u.UserData.Benefit != null)?.UserData.Benefit,
               ContractType =
                  orderedRecords.FirstOrDefault(u => u.UserData.ContractType != null)?.UserData.ContractType,
               ContractValidityDate = orderedRecords.FirstOrDefault(u => u.UserData.ContractValidityDate != null)
                  ?.UserData.ContractValidityDate,
               CostCenter = orderedRecords.FirstOrDefault(u => u.UserData.CostCenter != null)?.UserData.CostCenter,
               DateOfBirth = orderedRecords.FirstOrDefault(u => u.UserData.DateOfBirth != null)?.UserData.DateOfBirth,
               EndOfProbationPeriod = orderedRecords.FirstOrDefault(u => u.UserData.EndOfProbationPeriod != null)
                  ?.UserData.EndOfProbationPeriod,
               FirstName = orderedRecords.FirstOrDefault(u => u.UserData.FirstName != null)?.UserData.FirstName,
               ICEPhoneNumber = orderedRecords.FirstOrDefault(u => u.UserData.ICEPhoneNumber != null)?.UserData
                  .ICEPhoneNumber,
               LastName = orderedRecords.FirstOrDefault(u => u.UserData.LastName != null)?.UserData.LastName,
               Note = orderedRecords.FirstOrDefault(u => u.UserData.Note != null)?.UserData.Note,
               PersonalDataConfirmation = orderedRecords
                  .FirstOrDefault(u => u.UserData.PersonalDataConfirmation != null)?.UserData.PersonalDataConfirmation,
               PhoneNumber = orderedRecords.FirstOrDefault(u => u.UserData.PhoneNumber != null)?.UserData.PhoneNumber,
               Position = orderedRecords.FirstOrDefault(u => u.UserData.Position != null)?.UserData.Position,
               Rate = orderedRecords.FirstOrDefault(u => u.UserData.Rate != null)?.UserData.Rate,
               Salary = orderedRecords.FirstOrDefault(u => u.UserData.Salary != null)?.UserData.Salary,
               TerminationDate = orderedRecords.FirstOrDefault(u => u.UserData.TerminationDate != null)?.UserData
                  .TerminationDate,
               Title = orderedRecords.FirstOrDefault(u => u.UserData.Title != null)?.UserData.Title
            };
         });

         return freshestUserData.Where(x => !x.TerminationDate.HasValue || x.TerminationDate.Value.Date < DateTime.Now.Date).ToList();
      }

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