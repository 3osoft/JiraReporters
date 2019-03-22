using System;
using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;

namespace HRReports.Alerts
{
   public class EmploymentAnniversaryAlert : UserDataAlert
   {
      private readonly int _year;
      private static readonly TimeSpan AlertSpan = TimeSpan.FromDays(1);

      //todo this probably wont be start date, but some other date
      protected override string AlertText => $"{_year - UserData.StartDate.Value.Year}. vyrocie vo firme ({UserData.StartDate?.ToShortDateString()}) zajtra";

      public EmploymentAnniversaryAlert(UserData userData, int year) : base(userData)
      {
         _year = year;
      }

      public static List<EmploymentAnniversaryAlert> FromUserData(List<UserData> userData, DateTime currentDate)
      {
         //todo this probably wont be start date, but some other date
         //todo this will not work when employee start on 1.1 and crashes when he start on 29.2...
         return userData.Where(x => x.StartDate.HasValue && new DateTime(currentDate.Year, x.StartDate.Value.Month, x.StartDate.Value.Day) == currentDate.Date.Add(AlertSpan))
            .Select(x => new EmploymentAnniversaryAlert(x, currentDate.Year)).ToList();
      }
   }
}