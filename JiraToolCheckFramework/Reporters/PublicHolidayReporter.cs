using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JiraToolCheckFramework.JiraApi;
using RestSharp;

namespace JiraToolCheckFramework.Reporters
{
   public class PublicHolidayReporter : BaseReporter<List<PublicHoliday>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly string _apiKey;
      private readonly List<int> _yearsToReport;

      public PublicHolidayReporter(string apiKey, List<int> yearsToReport)
      {
         _apiKey = apiKey;
         _yearsToReport = yearsToReport;
      }

      protected override List<PublicHoliday> CalculateReportData()
      {
         Logger.Info("Getting public holidays");

         string publicHolidayType = "National holiday";
         List<PublicHoliday> result = new List<PublicHoliday>();
         RestClient restClient = new RestClient(new Uri("https://calendarific.com/api/v2/"));
         restClient.AddHandler("application/json", new DynamicJsonDeserializer());
         foreach (var year in _yearsToReport)
         {
            //?api_key={apiKey}&country=SK&year={year}

            var request = new RestRequest("calendar", Method.GET)
            {
               JsonSerializer = new NewtonsoftJsonSerializer()
            };

            request.AddQueryParameter("api_key", _apiKey);
            request.AddQueryParameter("country", "SK");
            request.AddQueryParameter("year", year.ToString());

            IRestResponse<dynamic> restResponse = restClient.Execute<dynamic>(request);
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
            Thread.Sleep(TimeSpan.FromSeconds(1));
         }

         return result; ;
      }

   }
}