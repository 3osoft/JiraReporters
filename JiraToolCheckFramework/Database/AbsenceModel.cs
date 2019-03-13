using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JiraToolCheckFramework.JiraApi;
using JiraToolCheckFramework.Utils;

namespace JiraToolCheckFramework.Database
{
   public class AbsenceModel
   {
      private const int WorkDayHours = 8;

      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int Id { get; set; }
      public string UserName { get; set; }
      public DateTime Date { get; set; }
      public decimal Hours { get; set; }
      public string AbsenceCategory { get; set; }

      public AbsenceCategory GetAbsenceCategory()
      {
         AbsenceCategory result;
         switch (AbsenceCategory)
         {
            case "Vacation":
               result = JiraApi.AbsenceCategory.Vacation;
               break;
            case "Illness":
               result = JiraApi.AbsenceCategory.Illness;
               break;
            case "Doctor":
               result = JiraApi.AbsenceCategory.Doctor;
               break;
            case "Doctor (Family)":
               result = JiraApi.AbsenceCategory.DoctorFamily;
               break;
            case "Personal leave":
               result = JiraApi.AbsenceCategory.PersonalLeave;
               break;
            default:
               result = JiraApi.AbsenceCategory.Unknown;
               break;
         }

         return result;
      }

      public static List<AbsenceModel> ToDatabaseModel(List<Absence> absences, List<PublicHoliday> holidays)
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
            
            //todo check for halfday absences

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

         var errorString = string.Join(Environment.NewLine,
            errors.Select(x => $"{x.Absence.IssueKey} {x.AbsenceErrorType}"));

         Console.WriteLine(errorString);

         return result;
      }
   }
}