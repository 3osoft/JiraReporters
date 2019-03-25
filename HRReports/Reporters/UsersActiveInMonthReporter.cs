using System;
using System.Collections.Generic;
using System.Linq;
using JiraReporterCore.Domain.Users;
using JiraReporterCore.Reporters;
using JiraReporterCore.Reporters.Users;

namespace HRReports.Reporters
{
   public class UsersActiveInMonthReporter : BaseReporter<List<UserData>>
   {
      private readonly FreshestUserDataReporter _freshestUserDataReporter;
      private readonly DateTime _monthStartDate;
      private readonly DateTime _monthEndDate;

      public UsersActiveInMonthReporter(FreshestUserDataReporter freshestUserDataReporter, DateTime monthStartDate, DateTime monthEndDate)
      {
         _freshestUserDataReporter = freshestUserDataReporter;
         _monthStartDate = monthStartDate;
         _monthEndDate = monthEndDate;
      }

      protected override List<UserData> CalculateReportData()
      {
         return _freshestUserDataReporter.Report()
            .Where(x => (!x.TerminationDate.HasValue || x.TerminationDate.Value.Date > _monthStartDate) && 
                        (!x.StartDate.HasValue || x.StartDate.Value.Date <= _monthEndDate))
            .ToList();
      }
   }
}