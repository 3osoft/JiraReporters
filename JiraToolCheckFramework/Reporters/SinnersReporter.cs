using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using JiraReporter.Configuration;
using JiraReporter.Gmail;
using JiraReporter.Reporters;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.GSheets;
using JiraToolCheckFramework.Sin;

namespace JiraToolCheckFramework.Reporters
{
   public class SinnersReporter : BaseReporter<object>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly UserReporter _userReporter;
      private readonly WorklogsReporter _worklogsReporter;
      private readonly AttendanceReporter _attendanceReporter;
      private readonly Config _config;
      private readonly DateTime _dateOfSin;

      public SinnersReporter(UserReporter userReporter, WorklogsReporter worklogsReporter, AttendanceReporter attendanceReporter, Config config, DateTime dateOfSin)
      {
         _userReporter = userReporter;
         _worklogsReporter = worklogsReporter;
         _attendanceReporter = attendanceReporter;
         _config = config;
         _dateOfSin = dateOfSin;
      }

      protected override object CalculateReportData()
      {
         Logger.Info("Resolving sinners");

         ResolveSinners();
         return null;
      }

      private void ResolveSinners()
      {
         var users = _userReporter.Report();
         var workLogs = _worklogsReporter.Report();
         var attendance = _attendanceReporter.Report();

         var worklogCountSinners = users.Join(workLogs, u => u.UserName, w => w.User, (u, w) => new
         {
            u.UserName,
            u.IsTracking,
            w.Date,
            w.Hours
         })
            .Where(uw => uw.Date.Equals(_dateOfSin) && uw.IsTracking)
            .GroupBy(uw => uw.UserName)
            .Select(guw =>
               new WorklogCountSinner
               {
                  TotalHours = guw.Sum(x => x.Hours),
                  WorklogCount = guw.Count(),
                  SinDate = _dateOfSin,
                  SinnerLogin = guw.Key
               })
            .Where(wcs => wcs.WorklogCount < WorklogCountSinner.CountThreshold);

         var longWorklogSinners = workLogs
            .Where(x => x.Date.Equals(_dateOfSin) && x.Hours > LongWorklogSinner.LongWorklogThreshold)
            .Join(users, w => w.User, u => u.UserName, (w, u) => new { w.Hours, w.User, u.IsTracking })
            .Where(x => x.IsTracking)
            .Select(x => new LongWorklogSinner
            {
               SinnerLogin = x.User,
               Hours = x.Hours,
               SinDate = _dateOfSin
            });

         var timeTrackedSinners = attendance
            .Where(x => x.Date.Equals(_dateOfSin) && (x.TotalHours < TimeTrackedSinner.LowHoursThreshold ||
                                                     x.TotalHours > TimeTrackedSinner.HighHoursThreshold))
            .Join(users, a => a.User, u => u.UserName,
               (a, u) => new { a.User, a.AbsenceTotal, a.TotalHours, a.HoursWorked, u.IsTracking })
            .Where(x => x.IsTracking)
            .Select(x => new TimeTrackedSinner
            {
               SinnerLogin = x.User,
               Absence = x.AbsenceTotal,
               TotalHours = x.TotalHours,
               TimeTracked = x.HoursWorked,
               SinDate = _dateOfSin
            });

         var sinners = new List<IEnumerable<Sinner>>
         {
            longWorklogSinners,
            timeTrackedSinners,
            worklogCountSinners
         };

         var sinnerSheet = new SinnersSheet(_config.SinnersSheetSettings);
         sinnerSheet.WriteSinners(sinners);

         SendMailForSinners(sinners, _config.SinnerNotifierGmailSettings, _dateOfSin);

      }
      private void SendMailForSinners(List<IEnumerable<Sinner>> sinners, GmailSettings settings, DateTime dateOfSin)
      {
         var fromAddress = new MailAddress(settings.FromAddress, settings.FromDisplayName);
         var toAddress = new MailAddress(settings.ToAddress, settings.ToDisplayName);
         string subject = $"Sinners for {dateOfSin.Date:d}";
         string body = GetMailBodyForSinners(sinners);

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
                  resultBuilder.AppendLine($"Ludia, ktory maju {oneCategorySinnerList.First().SinString}: ");
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