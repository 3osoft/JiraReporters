using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JiraToolCheckFramework.Database
{
   public class AttendanceModel
   {
      [Key, Column(Order = 0)]
      public DateTime Date { get; set; }
      [Key, Column(Order = 1)]
      public string User { get; set; }
      public decimal HoursWorked { get; set; }
      public decimal AbsenceIllness { get; set; }
      public decimal AbsenceVacation { get; set; }
      public decimal AbsenceDoctor { get; set; }
      public decimal AbsenceDoctorFamily { get; set; }
      public decimal AbsencePersonalLeave { get; set; }
      public decimal AbsenceTotal { get; set; }
      public decimal TotalHours { get; set; }

      public void CalculateTotals()
      {
         AbsenceTotal = AbsenceDoctorFamily + AbsencePersonalLeave + AbsenceVacation + AbsenceDoctor + AbsenceIllness;
         TotalHours = HoursWorked + AbsenceTotal;
      }
   }
}