using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JiraToolCheckFramework.Configuration;
using JiraToolCheckFramework.Database;
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
      public const string ConfigFilePath = "config.json";
      static void Main(string[] args)
      {
         Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));

         List<string> absenceStatusesToBeIgnored = new List<string>
         {
            "Canceled",
            "Rejected"
         };

         DateTime runStartDateTime = DateTime.Now;
         
         DateTime from = config.DateFrom ?? new DateTime(DateTime.Now.Year, 1, 1) ;
         DateTime till = config.DateTo ?? new DateTime(DateTime.Now.Year, 12, 31);

         JiraApiClient client = new JiraApiClient(config.JiraSettings);

         var userGSheet = new UserSheet(config.UsersSheetSettings);
         var userModels = userGSheet.GetUsers();
         var users = userModels.Select(x => x.UserName).ToList();

         var burnedDictionary = CalculateBudgetBurned(users, client);

         var budgetBurnedSheet = new ProjectTimeSpentSheet(config.ProjectTimeSpentSheetSettings);
         budgetBurnedSheet.WriteBudgetBurned(burnedDictionary);

         var initialsDictionary = userModels.ToDictionary(x => x.Initials, x => x.UserName);

         var allStatusAbsences = client.GetAbsences(initialsDictionary);
         var absences = allStatusAbsences.Where(x => !absenceStatusesToBeIgnored.Contains(x.Status));
         var holidays = GetPublicHolidays(Enumerable.Range(2016, till.Year - 2016 + 1).ToList(), config.PublicHolidayApiKey).Result;

         var absenceModels = AbsenceModel.ToDatabaseModel(absences.ToList(), holidays);

         var workLogs = GetWorklogs(users, client, from, till);

         var attendance = GetAttendances(users, absenceModels, workLogs, from, till);

         var timeGridSheet = new AttendanceGridSheet(config.AttendanceGridSheetSettings);
         timeGridSheet.WriteAttendance(attendance);

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

         ResolveSinners(userModels, absenceModels, workLogs, attendance, config);
         
         DateTime runEndDateTime = DateTime.Now;

         var runLogSheet = new RunLogSheet(config.RunLogSheetSettings);

         runLogSheet.WriteLog(runStartDateTime, runEndDateTime);
      }

      private static void ResolveSinners(List<UserModel> userModels, List<AbsenceModel> absenceModels,
         List<WorklogModel> workLogs,
         List<AttendanceModel> attendance, Config config)
      {
         var dateOfSin = DateTime.Today.Subtract(TimeSpan.FromDays(1)).Date;

         var worklogCountSinners = userModels.Join(absenceModels, u => u.UserName, a => a.UserName, (u, a) => new
            {
               u.UserName,
               u.IsTracking,
               a.Date,
               a.Hours
            })
            .Where(ua => ua.Date.Equals(dateOfSin) && ua.IsTracking)
            .GroupBy(ua => ua.UserName)
            .Select(gua =>
               new WorklogCountSinner
               {
                  TotalHours = gua.Sum(x => x.Hours),
                  WorklogCount = gua.Count(),
                  SinDate = dateOfSin,
                  SinnerLogin = gua.Key
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

         var sinnerSheet = new SinnersSheet(config.SinnersSheetSettings);
         sinnerSheet.WriteSinners(new List<IEnumerable<Sinner>>
         {
            longWorklogSinners,
            timeTrackedSinners,
            worklogCountSinners
         });
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
            { // todo type musi obsahovat National holiday
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
