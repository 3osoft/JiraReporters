namespace HRReports.Domain
{
   public class FoodStampData
   {
      public int Month { get; set; }
      public int Year { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string Title { get; set; }
      public int FoodStampCountEntitlement { get; set; }
      public int AdjustmentForAbsences { get; set; }
      public int IssuesFoodStampCount { get; set; }
   }
}