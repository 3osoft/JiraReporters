using System;

namespace JiraReporter.Domain
{
   public class Attendance
   {
      public DateTime Date { get; set; }
      public string User { get; set; }
      public decimal HoursWorked { get; set; }
      public decimal AbsenceIllness { get; set; }
      public decimal AbsenceVacation { get; set; }
      public decimal AbsenceDoctor { get; set; }
      public decimal AbsenceDoctorFamily { get; set; }
      public decimal AbsencePersonalLeave { get; set; }
      public decimal AbsenceTotal => AbsenceDoctorFamily + AbsencePersonalLeave + AbsenceVacation + AbsenceDoctor + AbsenceIllness;
      public decimal TotalHours => HoursWorked + AbsenceTotal;
   }
}