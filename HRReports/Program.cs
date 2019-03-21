using System;
using System.Collections.Generic;
using System.IO;
using HRReports.Configuration;
using HRReports.Domain;
using HRReports.GSheets;
using HRReports.Reporters;
using JiraReporterCore.Reporters.Writer;
using Newtonsoft.Json;

namespace HRReports
{
   class Program
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      public const string ConfigFilePath = "config.json";

      static void Main(string[] args)
      {
         AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
         {
            Logger.Fatal(eventArgs.ExceptionObject as Exception);
         };

         Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));

         var rawUserDataReporter = new RawUserDataReporter(new RawUserDataSheet(config.RawUsersSheetSettings));
         var currentUsersReporter = new CurrentUsersReporter(rawUserDataReporter);



         ReportWriter<List<UserData>> userDataWriter = new ReportWriter<List<UserData>>(currentUsersReporter, new CurrentUsersSheet(config.CurrentUsersSheetSettings));



         userDataWriter.Write();

      }
   }
}
