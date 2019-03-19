using System;

namespace JiraReporter.JiraApi
{
   public class JiraSettings
   {
      public Uri BaseUrl { get; set; } = new Uri("https://3osoft.atlassian.net/");
      public string Login { get; set; } = "adam.blasko";
      public string Password { get; set; } = "XXX";
   }
}