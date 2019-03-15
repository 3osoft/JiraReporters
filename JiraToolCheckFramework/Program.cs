using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Database;
using JiraToolCheckFramework.Gmail;
using JiraToolCheckFramework.GSheets;
using JiraToolCheckFramework.JiraApi;
using JiraToolCheckFramework.Sin;
using JiraToolCheckFramework.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace JiraToolCheckFramework
{
   class Program
   {
      private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

      public const string ConfigFilePath = "config.json";
      static void Main(string[] args)
      {
         AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
         {
            logger.Fatal(eventArgs.ExceptionObject as Exception);
         };

         logger.Info("Tool run started");
         Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));

         List<string> absenceStatusesToBeIgnored = new List<string>
         {
            "Canceled",
            "Rejected"
         };

         DateTime runStartDateTime = DateTime.Now;
         
         DateTime from = config.DateFrom ?? new DateTime(DateTime.Now.Year, 1, 1) ;
         DateTime till = config.DateTo ?? new DateTime(DateTime.Now.Year, 12, 31);

         logger.Info("Running for date range from {0} to {1}", from, till);

         JiraApiClient client = new JiraApiClient(config.JiraSettings);

         logger.Info("Getting users");
         var userGSheet = new UserSheet(config.UsersSheetSettings);
         var userModels = userGSheet.GetUsers();
         var users = userModels.Select(x => x.UserName).ToList();
         logger.Info("Found {0} users", users.Count);

         logger.Info("Calculating Time spent on projects");
         var burnedDictionary = CalculateBudgetBurned(users, client);

         var budgetBurnedSheet = new ProjectTimeSpentSheet(config.ProjectTimeSpentSheetSettings);
         budgetBurnedSheet.WriteBudgetBurned(burnedDictionary);

         var initialsDictionary = userModels.ToDictionary(x => x.Initials, x => x.UserName);

         logger.Info("Getting absences");
         var allStatusAbsences = client.GetAbsences(initialsDictionary);
         
         var absences = allStatusAbsences.Where(x => !absenceStatusesToBeIgnored.Contains(x.Status));
         logger.Info("Found {0} absences in all status, {1} in usable status", allStatusAbsences.Count(), absences.Count());
         logger.Info("Getting public holidays");
         var holidays = GetPublicHolidays(Enumerable.Range(2016, till.Year - 2016 + 1).ToList(), config.PublicHolidayApiKey).Result;

         var absenceModels = AbsenceModel.ToDatabaseModel(absences.ToList(), holidays);

         logger.Info("Getting worklogs");
         var workLogs = GetWorklogs(users, client, from, till);
         logger.Info("Calculating attencande");
         var attendance = GetAttendances(users, absenceModels, workLogs, from, till);

         logger.Info("Writing to time grid sheet");
         var timeGridSheet = new AttendanceGridSheet(config.AttendanceGridSheetSettings);
         timeGridSheet.WriteAttendance(attendance);

         logger.Info("Cleaning and filling database");
         System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseIfModelChanges<JiraToolDbContext>());
         using (JiraToolDbContext ctx = new JiraToolDbContext())
         {
            using (var transaction = ctx.Database.BeginTransaction())
            {
               ClearDatabase(ctx);
               ctx.Absences.AddRange(absenceModels);
               ctx.Worklogs.AddRange(workLogs);
               ctx.Attendance.AddRange(attendance);
               ctx.Users.AddRange(userModels);
               ctx.SaveChanges();
               transaction.Commit();
            }
         }

         logger.Info("Resolving sinners");
         ResolveSinners(userModels, workLogs, attendance, config);

         DateTime runEndDateTime = DateTime.Now;

         var runLogSheet = new RunLogSheet(config.RunLogSheetSettings);
         logger.Info("Writng run log");
         runLogSheet.WriteLog(runStartDateTime, runEndDateTime);
         logger.Info("Tool run finished");
      }

      private static void ResolveSinners(List<UserModel> userModels,
         List<WorklogModel> workLogs,
         List<AttendanceModel> attendance, Config config)
      {
         var dateOfSin = DateTime.Today.Subtract(TimeSpan.FromDays(1)).Date;

         var worklogCountSinners = userModels.Join(workLogs, u => u.UserName, w => w.User, (u, w) => new
            {
               u.UserName,
               u.IsTracking,
               w.Date,
               w.Hours
            })
            .Where(uw => uw.Date.Equals(dateOfSin) && uw.IsTracking)
            .GroupBy(uw => uw.UserName)
            .Select(guw =>
               new WorklogCountSinner
               {
                  TotalHours = guw.Sum(x => x.Hours),
                  WorklogCount = guw.Count(),
                  SinDate = dateOfSin,
                  SinnerLogin = guw.Key
               })
            .Where(wcs => wcs.WorklogCount < WorklogCountSinner.CountThreshold);

         var longWorklogSinners = workLogs
            .Where(x => x.Date.Equals(dateOfSin) && x.Hours > LongWorklogSinner.LongWorklogThreshold)
            .Join(userModels, w => w.User, u => u.UserName, (w, u) => new {w.Hours, w.User, u.IsTracking})
            .Where(x => x.IsTracking)
            .Select(x => new LongWorklogSinner
            {
               SinnerLogin = x.User,
               Hours = x.Hours,
               SinDate = dateOfSin
            });

         var timeTrackedSinners = attendance
            .Where(x => x.Date.Equals(dateOfSin) && (x.TotalHours < TimeTrackedSinner.LowHoursThreshold ||
                                                     x.TotalHours > TimeTrackedSinner.HighHoursThreshold))
            .Join(userModels, a => a.User, u => u.UserName,
               (a, u) => new {a.User, a.AbsenceTotal, a.TotalHours, a.HoursWorked, u.IsTracking})
            .Where(x => x.IsTracking)
            .Select(x => new TimeTrackedSinner
            {
               SinnerLogin = x.User,
               Absence = x.AbsenceTotal,
               TotalHours = x.TotalHours,
               TimeTracked = x.HoursWorked,
               SinDate = dateOfSin
            });

         var sinners = new List<IEnumerable<Sinner>>
         {
            longWorklogSinners,
            timeTrackedSinners,
            worklogCountSinners
         };

         var sinnerSheet = new SinnersSheet(config.SinnersSheetSettings);
         sinnerSheet.WriteSinners(sinners);

         SendMailForSinners(sinners, config.SinnerNotifierGmailSettings, dateOfSin);

      }

      private static void SendMailForSinners(List<IEnumerable<Sinner>> sinners, GmailSettings settings, DateTime dateOfSin)
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

      private static string GetMailBodyForSinners(List<IEnumerable<Sinner>> sinners)
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

      private static Dictionary<string, decimal> CalculateBudgetBurned(List<string> users, JiraApiClient client)
      {
         Dictionary<string, decimal> burnedPerProject = new Dictionary<string, decimal>();
         var worklogsForBudgetBurned = GetWorklogs(users, client, new DateTime(2016, 1, 1), DateTime.Now);

         foreach (var worklogModels in worklogsForBudgetBurned.GroupBy(x => x.ProjectKey))
         {
            burnedPerProject.Add(worklogModels.Key, worklogModels.Sum(x => x.Hours));
         }

         return burnedPerProject;
      }

      private static async Task<List<PublicHoliday>> GetPublicHolidays(List<int> years, string apiKey)
      {
         string publicHolidayType = "National holiday";
         List<PublicHoliday> result = new List<PublicHoliday>();
         RestClient restClient = new RestClient(new Uri("https://calendarific.com/api/v2/"));
         restClient.AddHandler("application/json", new DynamicJsonDeserializer());
         foreach (var year in years)
         {
            //?api_key={apiKey}&country=SK&year={year}
            
            var request = new RestRequest("calendar", Method.GET)
            {
               JsonSerializer = new NewtonsoftJsonSerializer()
            };

            request.AddQueryParameter("api_key", apiKey);
            request.AddQueryParameter("country", "SK");
            request.AddQueryParameter("year", year.ToString());

            IRestResponse<dynamic> restResponse = await restClient.ExecuteTaskAsync<dynamic>(request);
            restResponse.EnsureSuccessStatusCode();

            var holidaysResponseData = restResponse.Data;

            foreach (var holiday in holidaysResponseData.response.holidays)
            {
               var datetime = holiday.date.datetime;

               string[] types = holiday.type.ToObject<string[]>();

               if (types.Contains(publicHolidayType))
               {
                  result.Add(new PublicHoliday
                  {
                     Date = new DateTime((int)datetime.year, (int)datetime.month, (int)datetime.day),
                     Name = holiday.name
                  });
               }
            }

            //API limitation
            await Task.Delay(TimeSpan.FromSeconds(1));
         }

         return result;
      }

      private static List<AttendanceModel> GetAttendances(List<string> users, List<AbsenceModel> absences, List<WorklogModel> worklogs, DateTime from, DateTime to)
      {
         List<AttendanceModel> result = new List<AttendanceModel>();

         foreach (DateTime day in DateTimeUtils.EachDay(from, to))
         {
            foreach (var user in users)
            {
               var userDateAbsences = absences.Where(x => x.Date.Equals(day) && x.UserName.Equals(user)).ToList();
               var userDateWorklogs = worklogs.Where(x => x.Date.Equals(day) && x.User.Equals(user));

               var newAttendanceModel = new AttendanceModel
               {
                  Date = day,
                  User = user,
                  HoursWorked = userDateWorklogs.Sum(x => x.Hours),
                  AbsenceDoctor = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.Doctor).Sum(x => x.Hours),
                  AbsenceDoctorFamily = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.DoctorFamily).Sum(x => x.Hours),
                  AbsenceIllness = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.Illness).Sum(x => x.Hours),
                  AbsencePersonalLeave= userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.PersonalLeave).Sum(x => x.Hours),
                  AbsenceVacation = userDateAbsences.Where(x => x.GetAbsenceCategory() == AbsenceCategory.Vacation).Sum(x => x.Hours),
               };

               newAttendanceModel.CalculateTotals();
               result.Add(newAttendanceModel);
            }
         }

         return result;
      }

      private static List<WorklogModel> GetWorklogs(List<string> users, JiraApiClient client, DateTime from, DateTime till)
      {
         var result = new List<WorklogModel>();

         Worklogs worklogs = client.GetWorklogs(users, from, till);

         foreach (var user in users)
         {
            Timesheet personTimesheet = worklogs.GetTimesheet(user) ?? new Timesheet(user);
            result.AddRange(personTimesheet.Worklogs.Select(x => WorklogModel.FromWorklog(x, user)));
         }

         return result;
      }

      private static void ClearDatabase(JiraToolDbContext ctx)
      {
         ctx.Absences.RemoveRange(ctx.Absences);
         ctx.Worklogs.RemoveRange(ctx.Worklogs);
         ctx.Attendance.RemoveRange(ctx.Attendance);
         ctx.Users.RemoveRange(ctx.Users);
      }
   }
}
