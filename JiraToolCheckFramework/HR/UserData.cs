using System;

namespace JiraToolCheckFramework.HR
{
   public class UserData
   {
      public string Login { get; set; }
      public string Initials { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public bool? IsTracking { get; set; }
      public string Title { get; set; }
      public string Position { get; set; }
      public DateTime? StartDate { get; set; }
      public DateTime? EndOfProbationPeriod { get; set; }
      public DateTime? ContractValidityDate { get; set; }
      public DateTime? TerminationDate { get; set; }
      public string ContractType { get; set; }
      public string CostCenter { get; set; }
      public bool? PersonalDataConfirmation { get; set; }
      public decimal? Salary { get; set; }
      public decimal? Rate { get; set; }
      public string PhoneNumber { get; set; }
      public string ICEPhoneNumber { get; set; }
      public DateTime? DateOfBirth { get; set; }
      public string Benefit { get; set; }
      public string Note { get; set; }
   }
}