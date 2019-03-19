using System;
using RestSharp;

namespace JiraReporter.JiraApi
{
   public class RestSharpHttpException : Exception
   {
      public RestSharpHttpException(IRestResponse response) : base(
         GetMessageFromResponse(response))
      {
         Response = response;
      }

      public IRestResponse Response { get; private set; }

      private static string GetMessageFromResponse(IRestResponse response)
      {
         string result;

         if (response.StatusCode == 0)
         {
            result = string.Format("There was an HTTP problem: {0}", response.ErrorMessage);
         }
         else
         {
            result = string.Format("There was an HTTP problem. Code: {0} {2}{2}{2} Details: {1}", response.StatusCode, response.ErrorMessage, Environment.NewLine);
         }

         return result;
      }
   }
}