using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using HRReports.Alerts;
using JiraReporterCore.Configuration;
using JiraReporterCore.Gmail;

namespace HRReports.Gmail
{
   public class HrAlertGmail : WritableGmail<List<BaseAlert>>
   {
      private readonly DateTime _alertDate;

      public HrAlertGmail(GmailSettings settings, DateTime alertDate) : base(settings)
      {
         _alertDate = alertDate;
      }

      public override void Write(List<BaseAlert> data)
      {
         if (data.Any())
         {
            var fromAddress = new MailAddress(Settings.FromAddress, Settings.FromDisplayName);
            var toAddress = new MailAddress(Settings.ToAddress, Settings.ToDisplayName);
            var subject = $"Hr upozornenia za {_alertDate.Date:d}";
            var body = GetMailBodyForAlerts(data);

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

      private string GetMailBodyForAlerts(List<BaseAlert> alerts)
      {
         StringBuilder resultBuilder = new StringBuilder();

         foreach (var alertMailBody in alerts.Select(x => x.ToMailBody()))
         {
            resultBuilder.AppendLine(alertMailBody);
            resultBuilder.AppendLine("<br>");
         }
         
         return resultBuilder.ToString();

      }
   }
}