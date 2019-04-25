using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain;
using JiraReporterCore.JiraApi.Models;
using JiraReporterCore.Utils;

namespace JiraReporterCore.Reporters
{
   public class AbsenceReporter : BaseReporter<List<Absence>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
      private const int WorkDayHours = 8;

      private readonly PublicHolidayReporter _publicHolidayReporter;
      private readonly JiraAbsenceReporter _jiraAbsenceReporter;

      public AbsenceReporter(PublicHolidayReporter publicHolidayReporter, JiraAbsenceReporter jiraAbsenceReporter)
      {
         _publicHolidayReporter = publicHolidayReporter;
         _jiraAbsenceReporter = jiraAbsenceReporter;
      }

      protected override List<Absence> CalculateReportData()
      {
         Logger.Info("Calculating absences");
         var jiraAbsences = _jiraAbsenceReporter.Report();

         var holidays = _publicHolidayReporter.Report();
         var absences = ConvertJiraAbsencesToDomainAbsences(jiraAbsences, holidays);

         Logger.Info("Calculated {0} domain absences from {1} jira absences", absences.Count, jiraAbsences.Count);
         return absences;
      }

      private static List<Absence> ConvertJiraAbsencesToDomainAbsences(List<JiraAbsence> absences, List<PublicHoliday> holidays)
      {
         List<Absence> result = new List<Absence>();

         foreach (var absence in absences)
         {
            var totalAbsenceDuration = absence.DurationType == AbsenceDayHourEnum.Days
               ? absence.Duration * WorkDayHours
               : absence.Duration;

            var remainingAbsenceDuration = totalAbsenceDuration;

            var partialDuration = totalAbsenceDuration % WorkDayHours;
            remainingAbsenceDuration -= partialDuration;

            var isOneDayAbsence = absence.EndDate.Date == absence.StartDate.Date;

            bool hasPartialDayAtStart = false;
            bool hasPartialDayAtEnd = false;

            //we dont care about hafldays in same-day jiraAbsence
            if (!isOneDayAbsence)
            {
               hasPartialDayAtStart = absence.StartDate.Hour > 9;
               hasPartialDayAtEnd = absence.EndDate.Hour < 17;
            }

            foreach (var currentDay in DateTimeUtils.EachDay(absence.StartDate.Date, absence.EndDate.Date))
            {
               if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday && !holidays.Select(x => x.Date).Contains(currentDay))
               {
                  decimal newModelHours;

                  if (hasPartialDayAtStart && currentDay == absence.StartDate.Date)
                  {
                     newModelHours = partialDuration;
                  }
                  else if (hasPartialDayAtEnd && currentDay == absence.EndDate.Date)
                  {
                     newModelHours = partialDuration;
                  }
                  else if (isOneDayAbsence)
                  {
                     newModelHours = remainingAbsenceDuration > partialDuration ? remainingAbsenceDuration : partialDuration;
                     remainingAbsenceDuration = 0;
                  }
                  else
                  {
                     newModelHours = WorkDayHours;
                     remainingAbsenceDuration -= newModelHours;
                  }

                  result.Add(new Absence
                  {
                     AbsenceCategory = absence.AbsenceCategory,
                     Date = currentDay,
                     Hours = newModelHours,
                     UserName = absence.UserName
                  });

               }

            }
         }

         return result;
      }
   }
}