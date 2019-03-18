namespace JiraToolCheckFramework.HR
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
      public decimal Absences { get; set; }
      public decimal HoursWorked { get; set; }
      public decimal OvertimeHours { get; set; }

      public void CalculateOvertime()
      {
         OvertimeHours = (HoursWorked + Absences) - WorkHoursInMonth;
      }
   }
}