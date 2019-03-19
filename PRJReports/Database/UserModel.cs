using System.ComponentModel.DataAnnotations;

namespace PRJReports.Database
{
   public class UserModel
   {
      [Key]
      public string UserName { get; set; }
      public string Initials { get; set; }
      public bool IsTracking { get; set; }
   }
}