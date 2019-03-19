using System;
using System.ComponentModel.DataAnnotations.Schema;
using JiraReporterCore.JiraApi.Models;

namespace PRJReports.Database
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
               result = JiraReporterCore.JiraApi.Models.AbsenceCategory.Vacation;
               break;
            case "Illness":
               result = JiraReporterCore.JiraApi.Models.AbsenceCategory.Illness;
               break;
            case "Doctor":
               result = JiraReporterCore.JiraApi.Models.AbsenceCategory.Doctor;
               break;
            case "Doctor (Family)":
               result = JiraReporterCore.JiraApi.Models.AbsenceCategory.DoctorFamily;
               break;
            case "Personal leave":
               result = JiraReporterCore.JiraApi.Models.AbsenceCategory.PersonalLeave;
               break;
            default:
               result = JiraReporterCore.JiraApi.Models.AbsenceCategory.Unknown;
               break;
         }

         return result;
      }

      
   }
}