using System;
using System.ComponentModel.DataAnnotations.Schema;
using JiraReporter.JiraApi.Models;

namespace JiraToolCheckFramework.Database
{
   public class AbsenceModel
   {
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
               result = JiraReporter.JiraApi.Models.AbsenceCategory.Vacation;
               break;
            case "Illness":
               result = JiraReporter.JiraApi.Models.AbsenceCategory.Illness;
               break;
            case "Doctor":
               result = JiraReporter.JiraApi.Models.AbsenceCategory.Doctor;
               break;
            case "Doctor (Family)":
               result = JiraReporter.JiraApi.Models.AbsenceCategory.DoctorFamily;
               break;
            case "Personal leave":
               result = JiraReporter.JiraApi.Models.AbsenceCategory.PersonalLeave;
               break;
            default:
               result = JiraReporter.JiraApi.Models.AbsenceCategory.Unknown;
               break;
         }

         return result;
      }

      
   }
}