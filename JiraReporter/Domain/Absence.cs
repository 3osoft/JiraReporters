using System;
using JiraReporterCore.JiraApi.Models;

namespace JiraReporterCore.Domain
{
   public class Absence
   {
      public string UserName { get; set; }
      public DateTime Date { get; set; }
      public decimal Hours { get; set; }
      public string AbsenceCategory { get; set; }

      public override string ToString()
      {
         return $"{UserName}, {Date}, Hours: {Hours}, Category: {AbsenceCategory}";
      }

      public AbsenceCategory GetAbsenceCategory()
      {
         AbsenceCategory result;
         switch (AbsenceCategory)
         {
            case "Vacation":
               result = JiraApi.Models.AbsenceCategory.Vacation;
               break;
            case "Illness":
               result = JiraApi.Models.AbsenceCategory.Illness;
               break;
            case "Doctor":
               result = JiraApi.Models.AbsenceCategory.Doctor;
               break;
            case "Doctor (Family)":
               result = JiraApi.Models.AbsenceCategory.DoctorFamily;
               break;
            case "Personal leave":
               result = JiraApi.Models.AbsenceCategory.PersonalLeave;
               break;
            default:
               result = JiraApi.Models.AbsenceCategory.Unknown;
               break;
         }

         return result;
      }
   }
}