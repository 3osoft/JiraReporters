using JiraReporter.Domain;

namespace JiraToolCheckFramework.Database.Mappers
{
   public class AbsenceMapper
   {
      public static Absence ToDomain(AbsenceModel model)
      {
         return new Absence
         {
            Date = model.Date,
            Hours = model.Hours,
            UserName = model.UserName,
            AbsenceCategory = model.AbsenceCategory
         };
      }

      public static AbsenceModel ToModel(Absence domain)
      {
         return new AbsenceModel
         {
            Date = domain.Date,
            Hours = domain.Hours,
            UserName = domain.UserName,
            AbsenceCategory = domain.AbsenceCategory
         };
      }
   }
}