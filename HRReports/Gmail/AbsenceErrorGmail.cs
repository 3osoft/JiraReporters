using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using JiraReporterCore.Configuration;
using JiraReporterCore.Gmail;
using JiraReporterCore.JiraApi.Models;

namespace HRReports.Gmail
{
   public class AbsenceErrorGmail : WritableGmail<List<AbsenceError>>
   {
      public AbsenceErrorGmail(GmailSettings settings) : base(settings)
      {
      }

      public override void Write(List<AbsenceError> data)
      {
         if (data.Any())
         {
            var fromAddress = new MailAddress(Settings.FromAddress, Settings.FromDisplayName);
            var toAddress = new MailAddress(Settings.ToAddress, Settings.ToDisplayName);
            var subject = $"Chybne absencie";
            var body = GetMailBodyForAbsenceErrors(data);

            using (var message = new MailMessage(fromAddress, toAddress)
            {
               Subject = subject,
               Body = body,
               ReplyToList = { fromAddress.Address },
               IsBodyHtml = true
            })
            {
               GmailClient client = new GmailClient();
               client.SendMail(message);
            }

         }
      }

      private string GetMailBodyForAbsenceErrors(List<AbsenceError> errors)
      {
         StringBuilder resultBuilder = new StringBuilder();

         foreach (var errorMailBody in errors.Select(x => x.GetMailBody()))
         {
            resultBuilder.AppendLine(errorMailBody);
            resultBuilder.AppendLine("<br>");
         }

         return resultBuilder.ToString();

      }
   }
}