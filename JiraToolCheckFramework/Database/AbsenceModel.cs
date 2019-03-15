using System;
using System.ComponentModel.DataAnnotations.Schema;
using JiraToolCheckFramework.JiraApi;

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

      
   }
}