using System;
using HRReports.Domain;
using JiraReporterCore.Domain;
using JiraReporterCore.Domain.Users;

namespace HRReports.Alerts
{
   public abstract class UserDataAlert : BaseAlert
   {
      protected readonly UserData UserData;

      protected UserDataAlert(UserData userData)
      {
         UserData = userData;
      }

      public override string ToMailBody()
      {
         return $"<b>{UserData.FirstName} {UserData.LastName} </b> ma {AlertText}!";
      }
   }
}