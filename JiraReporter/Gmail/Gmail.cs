using JiraReporterCore.Configuration;

namespace JiraReporterCore.Gmail
{
   public abstract class Gmail
   {
      protected readonly GmailSettings Settings;
      protected readonly GmailClient Client;

      protected Gmail(GmailSettings settings)
      {
         Settings = settings;
         Client = new GmailClient();
      }
   }
}