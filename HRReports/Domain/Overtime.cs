namespace HRReports.Domain
{
   public class Overtime
   {
      public string Login { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string ContractType { get; set; }
      public string CostCenter { get; set; }
      public int WorkHoursInMonth { get; set; }
      public int Month { get; set; }
      public int Year { get; set; }
      public decimal Absences { get; set; }
      public decimal HoursWorked { get; set; }
      public decimal OvertimeHours => (HoursWorked + Absences) - WorkHoursInMonth;

      public override string ToString()
      {
         return $"{FirstName} {LastName}, {Year}/{Month}, Work hours: {WorkHoursInMonth}, " +
                $"Worked: {HoursWorked} h, Absences: {Absences}, Overtime: {OvertimeHours}";
      }
   }
}