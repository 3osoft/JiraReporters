using System;
using System.Collections.Generic;
using System.Linq;
using HRReports.Domain;

namespace HRReports.Alerts
{
   public class BirthdayAlert : UserDataAlert
   {
      private readonly int _year;
      private static readonly TimeSpan AlertSpan = TimeSpan.FromDays(1);
      protected override string AlertText => $"{_year - UserData.DateOfBirth.Value.Year}. narodeniny ({UserData.DateOfBirth?.ToShortDateString()}) zajtra";

      public BirthdayAlert(UserData userData, int year) : base(userData)
      {
         _year = year;
      }

      public static List<BirthdayAlert> FromUserData(List<UserData> userData, DateTime currentDate)
      {
         //todo this will not work when employee start on 1.1 and crashes when he start on 29.2...
         return userData.Where(x => x.DateOfBirth.HasValue && new DateTime(currentDate.Year, x.DateOfBirth.Value.Month, x.DateOfBirth.Value.Day) == currentDate.Date.Add(AlertSpan))
            .Select(x => new BirthdayAlert(x, currentDate.Year)).ToList();
      }
   }
}