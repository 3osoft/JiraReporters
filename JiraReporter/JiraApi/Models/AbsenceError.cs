﻿namespace JiraReporterCore.JiraApi.Models
{
   public class AbsenceError
   {
      public JiraAbsence JiraAbsence { get; set; }
      public AbsenceErrorType AbsenceErrorType { get; set; }

      public string AbsenceErrorString {
         get
         {
            string result = null;
            switch (AbsenceErrorType)
            {
               case AbsenceErrorType.MoreHoursInCalendarThanInDuration:
               case AbsenceErrorType.UnusedOrNoRemainingDuration:
                  result = "Dni/hodiny od startu po koniec absencie sa nezhoduju so zadanym trvanim";
                  break;
               case AbsenceErrorType.OneDayWithDurationOverWorkday:
                  result = "Jednodnova absencia dlhisa ako 8 hodin";
                  break;
               case AbsenceErrorType.PartialAtBothEnds:
                  result = "Absencia je ciastocna aj na zaciatku aj na konci (zle nastaveny cas)";
                  break;
               case AbsenceErrorType.CannotParseUserName:
                  result = "Nazov absencie je chybny a nebolo mozne zistit pouzivatela";
                  break;
            }
            return result;
         }}

      public string GetMailBody()
      {
         return $"Absencia <b><a href = \"{JiraAbsence.IssueLink}\">{JiraAbsence.IssueKey} {JiraAbsence.IssueName}</a></b> ({JiraAbsence.UserName}) ma chybu: <i>{AbsenceErrorString}</i>";
      }
   }

   public enum AbsenceErrorType
   {
      OneDayWithDurationOverWorkday,
      PartialAtBothEnds,
      MoreHoursInCalendarThanInDuration,
      UnusedOrNoRemainingDuration,
      CannotParseUserName
   }
}