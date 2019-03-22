using System;
using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;

namespace HRReports.Alerts
{
   public class EndOfProbationAlert : UserDataAlert
   {
      private static readonly TimeSpan AlertSpan = TimeSpan.FromDays(14);

      protected override string AlertText => $"koniec skusobnej doby ({UserData.EndOfProbationPeriod?.ToShortDateString()}) o {AlertSpan.TotalDays} dni";

      public EndOfProbationAlert(UserData userData) : base(userData)
      {
      }

      public static List<EndOfProbationAlert> FromUserData(List<UserData> userData, DateTime currentDate)
      {
         return userData.Where(x => x.EndOfProbationPeriod.HasValue && x.EndOfProbationPeriod.Value.Date == currentDate.Date.Add(AlertSpan))
            .Select(x => new EndOfProbationAlert(x)).ToList();
      }
   }
}