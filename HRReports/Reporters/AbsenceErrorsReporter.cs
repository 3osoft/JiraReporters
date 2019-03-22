using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.JiraApi.Models;
using JiraReporterCore.Reporters;
using JiraReporterCore.Utils;

namespace HRReports.Reporters
{
   public class AbsenceErrorsReporter : BaseReporter<List<AbsenceError>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
      private const int WorkDayHours = 8;

      private readonly JiraAbsenceReporter _jiraAbsenceReporter;
      private readonly PublicHolidayReporter _publicHolidayReporter;

      public AbsenceErrorsReporter(JiraAbsenceReporter jiraAbsenceReporter, PublicHolidayReporter publicHolidayReporter)
      {
         _jiraAbsenceReporter = jiraAbsenceReporter;
         _publicHolidayReporter = publicHolidayReporter;
      }

      protected override List<AbsenceError> CalculateReportData()
      {
         Logger.Info("Calculating absence errors");
         var jiraAbsences = _jiraAbsenceReporter.Report();

         var holidays = _publicHolidayReporter.Report();
         var errors = ValidateJiraAbsences(jiraAbsences, holidays);
         if (errors.Any())
         {
            var errorString = string.Join(Environment.NewLine,
               errors.Select(x => $"{x.JiraAbsence.IssueKey} {x.AbsenceErrorType}"));

            Logger.Warn("Found {0} errors in absences!", errors.Count);
            Logger.Warn(errorString);
         }
         else
         {
            Logger.Info("All absences are OK");
         }

         return errors;
      }

      private static List<AbsenceError> ValidateJiraAbsences(List<JiraAbsence> absences, List<PublicHoliday> holidays)
      {
         List<AbsenceError> errors = new List<AbsenceError>();

         foreach (var absence in absences)
         {
            var totalAbsenceDuration = absence.DurationType == AbsenceDayHourEnum.Days
               ? absence.Duration * WorkDayHours
               : absence.Duration;

            var remainingAbsenceDuration = totalAbsenceDuration;

            var partialDuration = totalAbsenceDuration % WorkDayHours;
            remainingAbsenceDuration -= partialDuration;

            var isOneDayAbsence = absence.EndDate.Date == absence.StartDate.Date;

            if (isOneDayAbsence && totalAbsenceDuration > WorkDayHours)
            {
               errors.Add(new AbsenceError
               {
                  JiraAbsence = absence,
                  AbsenceErrorType = AbsenceErrorType.OneDayWithDurationOverWorkday
               });
            }

            bool hasPartialDayAtStart = false;
            bool hasPartialDayAtEnd = false;

            //we dont care about hafldays in same-day jiraAbsence
            if (!isOneDayAbsence)
            {
               hasPartialDayAtStart = absence.StartDate.Hour > 9;
               hasPartialDayAtEnd = absence.EndDate.Hour < 17;
            }

            if (hasPartialDayAtStart && hasPartialDayAtEnd)
            {

               errors.Add(new AbsenceError
               {
                  JiraAbsence = absence,
                  AbsenceErrorType = AbsenceErrorType.PartialAtBothEnds
               });

               //throw new Exception($"JiraAbsence {jiraAbsence.IssueKey} has partial days at both start and end!");
            }

            foreach (var currentDay in DateTimeUtils.EachDay(absence.StartDate.Date, absence.EndDate.Date))
            {
               if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday && !holidays.Select(x => x.Date).Contains(currentDay))
               {
                  decimal newModelHours;

                  if (remainingAbsenceDuration < 0)
                  {

                     errors.Add(new AbsenceError
                     {
                        JiraAbsence = absence,
                        AbsenceErrorType = AbsenceErrorType.MoreHoursInCalendarThanInDuration
                     });

                     //throw new Exception($"More than all hours were trying to be used in jiraAbsence {jiraAbsence.IssueKey}");
                  }

                  if (hasPartialDayAtStart && currentDay == absence.StartDate.Date)
                  {
                  }
                  else if (hasPartialDayAtEnd && currentDay == absence.EndDate.Date)
                  {
                  }
                  else if (isOneDayAbsence)
                  {
                     remainingAbsenceDuration = 0;
                  }
                  else
                  {
                     newModelHours = WorkDayHours;
                     remainingAbsenceDuration -= newModelHours;
                  }
               }

            }
            if (remainingAbsenceDuration != 0)
            {
               errors.Add(new AbsenceError
               {
                  JiraAbsence = absence,
                  AbsenceErrorType = AbsenceErrorType.UnusedOrNoRemainingDuration
               });

               //throw new Exception($"Not all hours from jiraAbsence {jiraAbsence.IssueKey} were used");
            }
         }

         return errors;
      }
   }
}