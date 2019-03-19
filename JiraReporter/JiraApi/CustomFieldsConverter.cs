using System.Collections.Generic;

namespace JiraReporter.JiraApi
{
   public static class CustomFieldsConverter
   {
      public static readonly Dictionary<CustomFieldsEnum, string> CustomFields = new Dictionary<CustomFieldsEnum, string>
      {
         {CustomFieldsEnum.AbsEndDate, "customfield_11640"},
         {CustomFieldsEnum.AbsStartDate, "customfield_11639"},
         {CustomFieldsEnum.AbsActualNumberOfDaysHours, "customfield_11641"},
         {CustomFieldsEnum.AbsDaysHours, "customfield_11644"},
         {CustomFieldsEnum.AbsAbsenceCategory, "customfield_11643"},
      };

      public enum CustomFieldsEnum
      {
         AbsEndDate,
         AbsStartDate,
         AbsActualNumberOfDaysHours,
         AbsDaysHours,
         AbsAbsenceCategory
      }
   }
}
