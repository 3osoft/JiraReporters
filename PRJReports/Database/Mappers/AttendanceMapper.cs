using JiraReporterCore.Domain;

namespace PRJReports.Database.Mappers
{
   public class AttendanceMapper
   {
      public static Attendance ToDomain(AttendanceModel model)
      {
         return new Attendance
         {
            Date = model.Date,
            User = model.User,
            HoursWorked = model.HoursWorked,
            AbsenceDoctor = model.AbsenceDoctor,
            AbsenceDoctorFamily = model.AbsenceDoctorFamily,
            AbsencePersonalLeave = model.AbsencePersonalLeave,
            AbsenceVacation = model.AbsenceVacation,
            AbsenceIllness = model.AbsenceIllness
         };
      }

      public static AttendanceModel ToModel(Attendance domain)
      {
         return new AttendanceModel
         {
            Date = domain.Date,
            TotalHours = domain.TotalHours,
            User = domain.User,
            HoursWorked = domain.HoursWorked,
            AbsenceTotal = domain.AbsenceTotal,
            AbsenceDoctor = domain.AbsenceDoctor,
            AbsenceDoctorFamily = domain.AbsenceDoctorFamily,
            AbsencePersonalLeave = domain.AbsencePersonalLeave,
            AbsenceVacation = domain.AbsenceVacation,
            AbsenceIllness = domain.AbsenceIllness
         };
      }
   }
}