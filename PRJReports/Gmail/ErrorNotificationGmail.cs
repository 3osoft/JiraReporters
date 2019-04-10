using System;
using System.Net.Mail;
using JiraReporterCore.Configuration;
using JiraReporterCore.Gmail;

namespace PRJReports.Gmail
{
   public class ErrorNotificationGmail : WritableGmail<Exception>
   {
      public ErrorNotificationGmail(GmailSettings settings) : base(settings)
      {
      }

      public override void Write(Exception data)
      {
         var fromAddress = new MailAddress(Settings.FromAddress, Settings.FromDisplayName);
         var toAddress = new MailAddress(Settings.ToAddress, Settings.ToDisplayName);

         using (var message = new MailMessage(fromAddress, toAddress)
         {
            Subject = "Chyba - kontrola worklogov",
            Body = "Automaticka kontrola worklogov zlyhala, prosim, skontrolujte worklogy rucne.",
            ReplyToList = { fromAddress.Address },
            IsBodyHtml = true
         })
         {
            GmailClient client = new GmailClient();
            client.SendMail(message);
         }
      }
   }
}