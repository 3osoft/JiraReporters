﻿using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain;
using JiraReporterCore.Domain.Users;
using JiraReporterCore.Reporters;
using PRJReports.Sin;

namespace PRJReports.Reporters
{
   public class SinnersReporter : BaseReporter<List<IEnumerable<Sinner>>>
   {
      private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

      private readonly BaseReporter<List<UserData>> _userReporter;
      private readonly BaseReporter<List<Worklog>> _worklogsReporter;
      private readonly AttendanceReporter _attendanceReporter;
      private readonly DateTime _dateOfSin;

      public SinnersReporter(BaseReporter<List<UserData>> userReporter, BaseReporter<List<Worklog>> worklogsReporter, AttendanceReporter attendanceReporter, DateTime dateOfSin)
      {
         _userReporter = userReporter;
         _worklogsReporter = worklogsReporter;
         _attendanceReporter = attendanceReporter;
         _dateOfSin = dateOfSin;
      }

      protected override List<IEnumerable<Sinner>> CalculateReportData()
      {
         Logger.Info("Resolving sinners");

         var users = _userReporter.Report();
         var workLogs = _worklogsReporter.Report();
         var attendance = _attendanceReporter.Report();

         //var worklogCountSinners = users.Join(workLogs, u => u.UserName, w => w.User, (u, w) => new
         //{
         //   u.UserName,
         //   u.IsTracking,
         //   w.Date,
         //   w.Hours
         //})
         //   .Where(uw => uw.Date.Equals(_dateOfSin) && uw.IsTracking)
         //   .GroupBy(uw => uw.UserName)
         //   .Select(guw =>
         //      new WorklogCountSinner
         //      {
         //         TotalHours = guw.Sum(x => x.Hours),
         //         WorklogCount = guw.Count(),
         //         SinDate = _dateOfSin,
         //         SinnerLogin = guw.Key
         //      })
         //   .Where(wcs => wcs.WorklogCount < WorklogCountSinner.CountThreshold);

         var longWorklogSinners = workLogs
            .Where(x => x.Date.Equals(_dateOfSin) && x.Hours >= LongWorklogSinner.LongWorklogThreshold)
            .Join(users, w => w.User, u => u.Login, (w, u) => new { w.Hours, w.User, u.IsTracking })
            .Where(x => x.IsTracking.HasValue && x.IsTracking.Value)
            .Select(x => new LongWorklogSinner
            {
               SinnerLogin = x.User,
               Hours = x.Hours,
               SinDate = _dateOfSin
            });

         var timeTrackedSinners = attendance
            .Where(x => x.Date.Equals(_dateOfSin) && (x.TotalHours < TimeTrackedSinner.LowHoursThreshold ||
                                                     x.TotalHours > TimeTrackedSinner.HighHoursThreshold))
            .Join(users, a => a.User, u => u.Login,
               (a, u) => new { a.User, a.AbsenceTotal, a.TotalHours, a.HoursWorked, u.IsTracking })
            .Where(x => x.IsTracking.HasValue && x.IsTracking.Value)
            .Select(x => new TimeTrackedSinner
            {
               SinnerLogin = x.User,
               Absence = x.AbsenceTotal,
               TotalHours = x.TotalHours,
               TimeTracked = x.HoursWorked,
               SinDate = _dateOfSin
            });

         var noTimeTrackedSinners = attendance
            .Where(x => x.Date.Equals(_dateOfSin) && x.TotalHours == 0)
            .Join(users, a => a.User, u => u.Login,
               (a, u) => new { a.User,u.IsTracking })
            .Where(x => x.IsTracking.HasValue && x.IsTracking.Value)
            .Select(x => new NoTimeTrackedSinner
            {
               SinnerLogin = x.User,
               SinDate = _dateOfSin
            });

         var sinners = new List<IEnumerable<Sinner>>
         {
            longWorklogSinners,
            timeTrackedSinners,
            noTimeTrackedSinners
         };

         return sinners;
      }
   }
}