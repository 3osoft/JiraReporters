using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using JiraToolCheckFramework.JiraApi.Models;

namespace JiraToolCheckFramework.JiraApi
{
   public class JiraApiClientWithCache : JiraApiClient
   {
      private readonly string _cacheFolder;
      private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

      public JiraApiClientWithCache(string cacheFolder, JiraSettings jiraSettings) : base(jiraSettings)
      {
         _cacheFolder = cacheFolder;
         Directory.CreateDirectory(cacheFolder);
      }

      public override Worklogs GetWorklogs(IEnumerable<string> users, DateTime from, DateTime till)
      {
         Worklogs result;

         string cacheKey = string.Format("{0}_{1}_{2}", users.Count(), from.ToString("yyyyMMddTHHmmss"), till.ToString("yyyyMMddTHHmmss"));
         string cacheFilePath = Path.Combine(_cacheFolder, cacheKey);
         if (File.Exists(cacheFilePath))
         {
            using (FileStream cacheFile = new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read))
            {
               result = (Worklogs)_binaryFormatter.Deserialize(cacheFile);
            }               
         }
         else
         {
            result = base.GetWorklogs(users, from, till);
            using (FileStream cacheFile = new FileStream(cacheFilePath, FileMode.Create, FileAccess.Write))
            {
               _binaryFormatter.Serialize(cacheFile, result);
            }
         }

         return result;
      }
   }
}