using System;

namespace HRReports.Domain
{
   public class SalaryData
   {
      public int Month { get; set; }
      public int Year { get; set; }
      public string Login { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string Title { get; set; }
      public DateTime? StartDate { get; set; }
      public decimal? Salary { get; set; }
      public string Benefit { get; set; }
      public decimal VacationDays { get; set; }
      public decimal DoctorHours { get; set; }
      public decimal DoctorFamilyHours { get; set; }
      public decimal IllnessDays { get; set; }
   }
}