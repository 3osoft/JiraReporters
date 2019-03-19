using JiraReporterCore.Domain;

namespace PRJReports.Database.Mappers
{
   public class UserMapper
   {
      public static User ToDomain(UserModel model)
      {
         return new User
         {
            UserName = model.UserName,
            IsTracking = model.IsTracking,
            Initials = model.Initials
         };
      }

      public static UserModel ToModel(User domain)
      {
         return new UserModel
         {
            UserName = domain.UserName,
            IsTracking = domain.IsTracking,
            Initials = domain.Initials
         };
      }
   }
}