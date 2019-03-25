using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain.Users;

namespace HRReports.Alerts
{
   public class EmploymentAnniversaryAlert : UserDataAlert
   {
      private readonly int _year;
      private static readonly TimeSpan AlertSpan = TimeSpan.FromDays(1);

      protected override string AlertText => $"{_year - UserData.WorkAnniversaryDate.Value.Year}. vyrocie vo firme ({UserData.WorkAnniversaryDate?.ToShortDateString()}) zajtra";

      public EmploymentAnniversaryAlert(UserData userData, int year) : base(userData)
      {
         _year = year;
      }

      public static List<EmploymentAnniversaryAlert> FromUserData(List<UserData> userData, DateTime currentDate)
      {
         //todo this will not work when employee start on 1.1 and crashes when he start on 29.2...
         return userData.Where(x => x.WorkAnniversaryDate.HasValue && new DateTime(currentDate.Year, x.WorkAnniversaryDate.Value.Month, x.WorkAnniversaryDate.Value.Day) == currentDate.Date.Add(AlertSpan))
            .Select(x => new EmploymentAnniversaryAlert(x, currentDate.Year)).ToList();
      }
   }
}