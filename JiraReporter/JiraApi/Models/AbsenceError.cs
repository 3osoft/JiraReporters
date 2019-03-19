namespace JiraReporterCore.JiraApi.Models
{
   public class AbsenceError
   {
      public JiraAbsence JiraAbsence { get; set; }
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