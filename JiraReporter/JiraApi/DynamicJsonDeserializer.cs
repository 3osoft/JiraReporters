﻿using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;

namespace JiraReporterCore.JiraApi
{
   public class DynamicJsonDeserializer : IDeserializer
   {
      public string RootElement { get; set; }
      public string Namespace { get; set; }
      public string DateFormat { get; set; }

      public T Deserialize<T>(IRestResponse response)
      {
         return JsonConvert.DeserializeObject<dynamic>(response.Content);
      }
   }
}