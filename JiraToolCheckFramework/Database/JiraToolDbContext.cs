using System.Data.Entity;

namespace JiraToolCheckFramework.Database
{
   class JiraToolDbContext : DbContext
   {
      public JiraToolDbContext(): base("JiraToolDbContext")
      {
      }
      public DbSet<WorklogModel> Worklogs { get; set; }
      public DbSet<AbsenceModel> Absences { get; set; }
      public DbSet<AttendanceModel> Attendance { get; set; }
      public DbSet<UserModel> Users { get; set; }
   }
}
