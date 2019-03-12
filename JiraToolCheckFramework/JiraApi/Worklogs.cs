using System;
using System.Collections.Generic;

namespace JiraToolCheckFramework.JiraApi
{
   [Serializable]
   public class Worklogs
   {
      private readonly Dictionary<string, Timesheet> _userToTimesheet =
         new Dictionary<string, Timesheet>();

      public Timesheet GetTimesheet(string user)
      {
         Timesheet result = null;
         if (_userToTimesheet.ContainsKey(user))
         {
            result = _userToTimesheet[user];
         }

         return result;
      }

      public void AddWorklogForUser(string user, Worklog worklog)
      {
         //Timesheet timesheet = _userToTimesheet.GetOrCreate(user, new Timesheet(user));
         bool exists = _userToTimesheet.TryGetValue(user, out var timesheet);

         if (!exists)
         {
            timesheet = new Timesheet(user);
            _userToTimesheet.Add(user, timesheet);
         }

         timesheet.AddWorklog(worklog);
      }
   }
}