using System.ComponentModel.DataAnnotations;

namespace JiraToolCheckFramework.Database
{
   public class UserModel
   {
      [Key]
      public string UserName { get; set; }

      public string Initials { get; set; }
   }
}