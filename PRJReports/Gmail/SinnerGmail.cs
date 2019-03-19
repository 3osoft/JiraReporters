using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using JiraReporterCore.Configuration;
using JiraReporterCore.Gmail;
using PRJReports.Sin;

namespace PRJReports.Gmail
{
   public class SinnerGmail : WritableGmail<List<IEnumerable<Sinner>>>
   {
      private readonly DateTime _dateOfSin;
      public SinnerGmail(GmailSettings settings, DateTime dateOfSin) : base(settings)
      {
         _dateOfSin = dateOfSin;
      }

      public override void Write(List<IEnumerable<Sinner>> data)
      {
         var fromAddress = new MailAddress(Settings.FromAddress, Settings.FromDisplayName);
         var toAddress = new MailAddress(Settings.ToAddress, Settings.ToDisplayName);
         string subject = $"Sinners for {_dateOfSin.Date:d}";
         string body = GetMailBodyForSinners(data);

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

      private string GetMailBodyForSinners(List<IEnumerable<Sinner>> sinners)
      {
         StringBuilder resultBuilder = new StringBuilder();

         if (sinners.Any(x => x.Any()))
         {
            foreach (var oneCategorySinner in sinners)
            {
               var oneCategorySinnerList = oneCategorySinner.ToList();
               if (oneCategorySinnerList.Any())
               {
                  resultBuilder.AppendLine("<b>");
                  resultBuilder.AppendLine($"Ludia, ktori maju {oneCategorySinnerList.First().SinString}: ");
                  resultBuilder.AppendLine("</b>");
                  resultBuilder.AppendLine("<br>");
                  foreach (var sinner in oneCategorySinnerList)
                  {
                     resultBuilder.AppendLine(sinner.ToMailString());
                     resultBuilder.AppendLine("<br>");
                  }
                  resultBuilder.AppendLine("<br>");
               }
            }
         }
         else
         {
            resultBuilder.Append("No sinners!");
            //todo add a meme
            //todo maybe count consecutive days
         }

         return resultBuilder.ToString();

      }
   }
}