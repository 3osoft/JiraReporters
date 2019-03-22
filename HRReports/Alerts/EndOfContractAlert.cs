using System;
using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;

namespace HRReports.Alerts
{
   public class EndOfContractAlert : UserDataAlert
   {
      private static readonly TimeSpan AlertSpan = TimeSpan.FromDays(60);

      protected override string AlertText => $"koniec pracovnej zmluvy ({UserData.ContractValidityDate?.ToShortDateString()}) o {AlertSpan.TotalDays} dni";

      public EndOfContractAlert(UserData userData) : base(userData)
      {
      }

      public static List<EndOfContractAlert> FromUserData(List<UserData> userData, DateTime currentDate)
      {
         return userData.Where(x => x.ContractValidityDate.HasValue && x.ContractValidityDate.Value.Date == currentDate.Date.Add(AlertSpan))
            .Select(x => new EndOfContractAlert(x)).ToList();
      }
   }
}