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
         
         DateTime from = config.DateFrom;
         DateTime till = DateTime.Now;

         JiraApiClient client = new JiraApiClient(config.JiraSettings);
         
         var userGSheet = new UserSheet(config.GoogleSheetsSettings);
         var userModels = userGSheet.GetUsers();
         var users = userModels.Select(x => x.UserName).ToList();

         var initialsDictionary = userModels.ToDictionary(x => x.Initials, x => x.UserName);

         var allStatusAbsences = client.GetAbsences(initialsDictionary);
         var absences = allStatusAbsences.Where(x => !absenceStatusesToBeIgnored.Contains(x.Status));
         var holidays = GetPublicHolidays(Enumerable.Range(2016, (till.Year - 2016) + 1).ToList(), config.PublicHolidayApiKey).Result;

         var absenceModels = AbsenceModel.ToDatabaseModel(absences.ToList(), holidays);

         var workLogs = GetWorklogs(users, client, from, till);

         var attendance = GetAttendances(users, absenceModels, workLogs, from, till);

         var timeGridSheet = new TimeGridSheet(config.GoogleSheetsSettings);
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
      }

      private static async Task<List<PublicHoliday>> GetPublicHolidays(List<int> years, string apiKey)
      {
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
               result.Add(new PublicHoliday
               {
                  Date = new DateTime((int) datetime.year, (int) datetime.month, (int)datetime.day),
                  Name = holiday.name
               });
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
