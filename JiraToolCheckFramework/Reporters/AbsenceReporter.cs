﻿using System;
using System.Collections.Generic;
using System.Linq;
using JiraToolCheckFramework.Database;
using JiraToolCheckFramework.JiraApi;
using JiraToolCheckFramework.JiraApi.Models;
using JiraToolCheckFramework.Utils;

namespace JiraToolCheckFramework.Reporters
{
   public class AbsenceReporter : BaseReporter<List<AbsenceModel>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
      private const int WorkDayHours = 8;

      private readonly List<string> _absenceStatusesToBeIgnored;
      private readonly UserReporter _userReporter;
      private readonly JiraApiClient _jiraApiClient;
      private readonly PublicHolidayReporter _publicHolidayReporter;

      public AbsenceReporter(PublicHolidayReporter publicHolidayReporter, UserReporter userReporter, JiraApiClient jiraApiClient)
      {
         _publicHolidayReporter = publicHolidayReporter;
         _jiraApiClient = jiraApiClient;
         _userReporter = userReporter;
         _absenceStatusesToBeIgnored = new List<string>
         {
            "Canceled",
            "Rejected"
         };
      }

      protected override List<AbsenceModel> CalculateReportData()
      {
         Logger.Info("Getting absences");

         var initialsDictionary = _userReporter.Report().ToDictionary(x => x.Initials, x => x.UserName);
         var allStatusAbsences = _jiraApiClient.GetAbsences(initialsDictionary);
         var absences = allStatusAbsences.Where(x => !_absenceStatusesToBeIgnored.Contains(x.Status));

         Logger.Info("Found {0} absences in all status, {1} in usable status", allStatusAbsences.Count(), absences.Count());
         
         var holidays = _publicHolidayReporter.Report();

         return ConverJiraAbsencesToModelAbsences(absences.ToList(), holidays);
      }

      private static List<AbsenceModel> ConverJiraAbsencesToModelAbsences(List<Absence> absences, List<PublicHoliday> holidays)
      {
         List<AbsenceModel> result = new List<AbsenceModel>();
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
                  Absence = absence,
                  AbsenceErrorType = AbsenceErrorType.OneDayWithDurationOverWorkday
               });
               //throw new Exception($"Absence {absence.IssueKey} is for the same day with duration {totalAbsenceDuration}");
            }

            bool hasPartialDayAtStart = false;
            bool hasPartialDayAtEnd = false;

            //we dont care about hafldays in same-day absence
            if (!isOneDayAbsence)
            {
               hasPartialDayAtStart = absence.StartDate.Hour > 9;
               hasPartialDayAtEnd = absence.EndDate.Hour < 17;
            }

            if (hasPartialDayAtStart && hasPartialDayAtEnd)
            {

               errors.Add(new AbsenceError
               {
                  Absence = absence,
                  AbsenceErrorType = AbsenceErrorType.PartialAtBothEnds
               });

               //throw new Exception($"Absence {absence.IssueKey} has partial days at both start and end!");
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
                        Absence = absence,
                        AbsenceErrorType = AbsenceErrorType.MoreHoursInCalendarThanInDuration
                     });

                     //throw new Exception($"More than all hours were trying to be used in absence {absence.IssueKey}");
                  }

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

                  result.Add(new AbsenceModel
                  {
                     AbsenceCategory = absence.AbsenceCategory,
                     Date = currentDay,
                     Hours = newModelHours,
                     UserName = absence.Name
                  });

               }

            }
            if (remainingAbsenceDuration != 0)
            {
               errors.Add(new AbsenceError
               {
                  Absence = absence,
                  AbsenceErrorType = AbsenceErrorType.UnusedOrNoRemainingDuration
               });

               //throw new Exception($"Not all hours from absence {absence.IssueKey} were used");
            }
         }

         if (errors.Any())
         {
            var errorString = string.Join(Environment.NewLine,
               errors.Select(x => $"{x.Absence.IssueKey} {x.AbsenceErrorType}"));

            Logger.Warn("Found {0} errors in absences!", errors.Count);
            Logger.Warn(errorString);
         }

         return result;
      }
   }
}