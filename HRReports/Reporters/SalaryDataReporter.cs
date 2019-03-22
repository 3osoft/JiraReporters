using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using HRReports.Domain;
using JiraReporterCore.JiraApi.Models;
using JiraReporterCore.Reporters;
using JiraReporterCore.Utils;

namespace HRReports.Reporters
{
   public class SalaryDataReporter : BaseReporter<List<SalaryData>>
   {
      private static readonly ContractType[] SalariedContractTypes =
         {ContractType.Employee, ContractType.PartTimeEmployee};

      private const string IllnessAbsenceCategoryString = "Illness";

      private readonly CurrentUsersReporter _currentUsersReporter;
      private readonly AttendanceReporter _attendanceReporter;
      private readonly JiraAbsenceReporter _jiraAbsenceReporter;
      private readonly int _year;
      private readonly int _month;

      public SalaryDataReporter(CurrentUsersReporter currentUsersReporter, AttendanceReporter attendanceReporter, JiraAbsenceReporter jiraAbsenceReporter, int year, int month)
      {
         _currentUsersReporter = currentUsersReporter;
         _attendanceReporter = attendanceReporter;
         _year = year;
         _month = month;
         _jiraAbsenceReporter = jiraAbsenceReporter;
      }

      protected override List<SalaryData> CalculateReportData()
      {
         var jiraAbsences = _jiraAbsenceReporter.Report();

         var users = _currentUsersReporter.Report();
         var salariedUsers = users.Where(x => SalariedContractTypes.Contains(x.GetContractType()));
         var relevantAttendance = _attendanceReporter.Report()
            .Where(x => x.Date.Year == _year && x.Date.Month == _month)
            .GroupBy(x => x.User)
            .Select(x => new
            {
               User = x.Key,
               HoursWorkedTotal = x.Sum(y => y.HoursWorked),
               AbsenceVacationTotal = x.Sum(y => y.AbsenceVacation),
               AbsenceDoctorTotal = x.Sum(y => y.AbsenceDoctor),
               AbsenceDoctorFamilyTotal = x.Sum(y => y.AbsenceDoctorFamily),
            })
            .Join(salariedUsers, a => a.User, u => u.Login,
               (a, u) => new
               {
                  u.Login,
                  u.Title,
                  u.StartDate,
                  u.Benefit,
                  u.FirstName,
                  u.LastName,
                  u.Salary,
                  u.Rate, //TODO based on the data setup, Rate might not be needed
                  ContractType = u.GetContractType(),
                  a.HoursWorkedTotal,
                  a.AbsenceVacationTotal,
                  a.AbsenceDoctorTotal,
                  a.AbsenceDoctorFamilyTotal
               });

         var result = relevantAttendance.Select(x =>
         {
            decimal? salary = null;
            if (x.ContractType == ContractType.Employee)
            {
               salary = x.Salary;
            }
            else if (x.ContractType == ContractType.PartTimeEmployee)
            {
               //TODO based on the data setup, Rate might not be needed and Salary needs to be used
               salary = x.Rate * x.HoursWorkedTotal;
            }

            return new SalaryData
            {
               Year = _year,
               Month = _month,
               Title = x.Title,
               StartDate = x.StartDate,
               Benefit = x.Benefit,
               FirstName = x.FirstName,
               Login = x.Login,
               LastName = x.LastName,
               DoctorFamilyHours = x.AbsenceDoctorFamilyTotal,
               DoctorHours = x.AbsenceDoctorTotal,
               VacationDays = x.AbsenceVacationTotal / 8,
               Salary = salary,
               IllnessDays = CalculateIllnessDays(x.Login, jiraAbsences)
            };
         });

         return result.ToList();
      }

      private decimal CalculateIllnessDays(string userLogin, List<JiraAbsence> jiraAbsences)
      {
         decimal result = 0;
         var relevantAbsences = jiraAbsences.Where(x => x.Name == userLogin
                                                        && x.AbsenceCategory == IllnessAbsenceCategoryString
                                                        && (x.StartDate.Month == _month && x.StartDate.Year == _year ||
                                                            x.EndDate.Month == _month && x.EndDate.Year == _year));

         foreach (var absence in relevantAbsences)
         {
            foreach (var day in DateTimeUtils.EachDay(absence.StartDate, absence.EndDate))
            {
               if (day.Year == _year && day.Month == _month)
               {
                  result++;
               }
            }
         }

         return result;
      }
   }
}