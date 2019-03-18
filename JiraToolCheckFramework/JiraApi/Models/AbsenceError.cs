namespace JiraToolCheckFramework.JiraApi.Models
{
   public class AbsenceError
   {
      public Absence Absence { get; set; }
      public AbsenceErrorType AbsenceErrorType { get; set; }
   }

   public enum AbsenceErrorType
   {
      OneDayWithDurationOverWorkday,
      PartialAtBothEnds,
      MoreHoursInCalendarThanInDuration,
      UnusedOrNoRemainingDuration
   }
}