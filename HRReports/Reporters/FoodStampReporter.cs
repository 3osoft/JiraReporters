using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;
using JiraReporterCore.Reporters;

namespace HRReports.Reporters
{
   public class FoodStampReporter : BaseReporter<List<FoodStampData>>
   {
      private const int HoursForAbsenceAdjustment = 4;

      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly MonthWorkHoursReporter _monthWorkHoursReporter;
      private readonly AbsenceReporter _absenceReporter;
      private readonly CurrentUsersReporter _currentUsersReporter;
      private readonly int _year;
      private readonly int _month;

      public FoodStampReporter(MonthWorkHoursReporter monthWorkHoursReporter, AbsenceReporter absenceReporter, CurrentUsersReporter currentUsersReporter, int year, int month)
      {
         _monthWorkHoursReporter = monthWorkHoursReporter;
         _absenceReporter = absenceReporter;
         _currentUsersReporter = currentUsersReporter;
         _year = year;
         _month = month;
      }

      protected override List<FoodStampData> CalculateReportData()
      {
         Logger.Info("Calculating food stamp data");
         var entitledUsers = _currentUsersReporter.Report().Where(x => x.GetContractType() == ContractType.Employee);
         var monthWorkDays = _monthWorkHoursReporter.Report() / 8;
         var absences = _absenceReporter.Report().Where(x => x.Date.Month == _month && x.Date.Year == _year).ToList();

         var result = entitledUsers.Select(x =>
         {
            var absencesForAdjustment =
               absences.Count(a => a.UserName == x.Login && a.Hours >= HoursForAbsenceAdjustment);
            return new FoodStampData
            {
               Month = _month,
               Year = _year,
               FirstName = x.FirstName,
               LastName = x.LastName,
               Title = x.Title,
               FoodStampCountEntitlement = monthWorkDays,
               AdjustmentForAbsences = absencesForAdjustment,
               IssuesFoodStampCount = monthWorkDays - absencesForAdjustment
            };
         });

         return result.ToList();
      }
   }
}