using System;

namespace JiraReporterCore.Domain.Users
{
   public class RawUserData
   {
      public DateTime RecordDate { get; set; }
      public UserData UserData { get; set; }
   }
}