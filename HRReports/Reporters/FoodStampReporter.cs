﻿using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Domain.Users;
using JiraReporterCore.Reporters;

namespace HRReports.Reporters
{
   public class FoodStampReporter : BaseReporter<List<FoodStampData>>
   {
      private const int HoursForAbsenceAdjustment = 4;

      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly MonthWorkHoursReporter _monthWorkHoursReporter;
      private readonly AbsenceReporter _absenceReporter;
      private readonly UsersActiveInMonthReporter _usersActiveInMonthReporter;
      private readonly int _year;
      private readonly int _month;
      private readonly int _previousYear;
      private readonly int _previousMonth;

      public FoodStampReporter(MonthWorkHoursReporter monthWorkHoursReporter, 
                               AbsenceReporter absenceReporter, 
                               UsersActiveInMonthReporter usersActiveInMonthReporter, 
                               int year, 
                               int month, 
                               int previousYear, 
                               int previousMonth)
      {
         _monthWorkHoursReporter = monthWorkHoursReporter;
         _absenceReporter = absenceReporter;
         _usersActiveInMonthReporter = usersActiveInMonthReporter;
         _year = year;
         _month = month;
         _previousYear = previousYear;
         _previousMonth = previousMonth;
      }

      protected override List<FoodStampData> CalculateReportData()
      {
         Logger.Info("Calculating food stamp data");
         
         var entitledUsers = _usersActiveInMonthReporter.Report()
            .Where(x => x.GetContractType() == ContractType.Employee);

         var monthWorkDays = _monthWorkHoursReporter.Report() / 8;

         var absences = _absenceReporter.Report()
            .Where(x => x.Date.Month == _previousMonth && x.Date.Year == _previousYear)
            .ToList();

         var result = entitledUsers.Select(x =>
         {
            var absencesForAdjustment = absences.Count(a => a.UserName == x.Login 
                                                            && a.Hours >= HoursForAbsenceAdjustment);
            return new FoodStampData
            {
               Month = _month,
               Year = _year,
               FirstName = x.FirstName,
               LastName = x.LastName,
               FoodStampCountEntitlement = monthWorkDays,
               AdjustmentForAbsences = absencesForAdjustment
            };
         });

         return result.ToList();
      }
   }
}