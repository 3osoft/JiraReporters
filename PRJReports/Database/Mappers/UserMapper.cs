using JiraReporterCore.Domain.Users;

namespace PRJReports.Database.Mappers
{
   public class UserMapper
   {
      public static UserData ToDomain(UserModel model)
      {
         return new UserData
         {
            Login = model.UserName,
            IsTracking = model.IsTracking,
            Initials = model.Initials
         };
      }

      public static UserModel ToModel(UserData domain)
      {
         return new UserModel
         {
            UserName = domain.Login,
            IsTracking = domain.IsTracking.HasValue && domain.IsTracking.Value,
            Initials = domain.Initials
         };
      }
   }
}