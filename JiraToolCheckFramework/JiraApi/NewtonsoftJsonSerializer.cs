using System.IO;
using Newtonsoft.Json;
using RestSharp.Serializers;

namespace JiraToolCheckFramework.JiraApi
{
   public class NewtonsoftJsonSerializer : ISerializer
   {
      private readonly Newtonsoft.Json.JsonSerializer _serializer;

      public NewtonsoftJsonSerializer()
      {
         ContentType = "application/json";
         _serializer = new Newtonsoft.Json.JsonSerializer
         {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
         };
         _serializer.Converters.Add(new JiraDateTimeJsonConverter());
      }

      public string Serialize(object obj)
      {
         using (var stringWriter = new StringWriter())
         {
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
               jsonTextWriter.Formatting = Formatting.Indented;
               jsonTextWriter.QuoteChar = '"';

               _serializer.Serialize(jsonTextWriter, obj);

               var result = stringWriter.ToString();
               return result;
            }
         }
      }

      public string DateFormat { get; set; }
      public string RootElement { get; set; }
      public string Namespace { get; set; }
      public string ContentType { get; set; }
   }
}