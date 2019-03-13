using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JiraToolCheckFramework.JiraApi
{
   internal static class AbsenceMapper
   {
      public static Absence MapAbsence(dynamic item, Dictionary<string, string> userNameInitialsDictionary)
      {
         var endDateField = CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsEndDate];
         var startDateField = CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsStartDate];
         var actualNumberOfDaysHoursField = CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsActualNumberOfDaysHours];
         var daysHourField = CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsDaysHours];
         var absenceCategoryField = CustomFieldsConverter.CustomFields[CustomFieldsConverter.CustomFieldsEnum.AbsAbsenceCategory];

         if (item != null)
         {
            Absence absence = new Absence
            {
               IssueKey = item.key,
               Status = item.fields.status.name,
               Name = NameVerification(item.fields.summary.ToString().Trim(), item.fields.creator.name.ToString().Trim(), userNameInitialsDictionary),
               CreatedDate = item.fields.created,
               StartDate = item.fields[startDateField],
               EndDate = item.fields[endDateField],
               Duration = decimal.Parse(item.fields[actualNumberOfDaysHoursField].ToString()),
               DurationType = item.fields[daysHourField].value.ToString().Equals("Days") ? AbsenceDayHourEnum.Days : AbsenceDayHourEnum.Hours,
               AbsenceCategory = item.fields[absenceCategoryField].value.ToString()
            };
            return absence;
         }
         return null;
      }

      private static string NameVerification(string summary, string creator, Dictionary<string, string> userNameInitialsDictionary)
      {
         string result;
         if (creator != "removed")
         {
            //string trimmedSummary = summary.Trim();
            if (!summary.Contains("-"))
            {
               result = "Error";
            }
            else
            {
               string summaryInitials = RemoveDiacritics(summary.Substring(0, summary.IndexOf("-", StringComparison.Ordinal))).Trim();
               //string summaryInitials = RemoveDiacritics();

               result = userNameInitialsDictionary.ContainsKey(summaryInitials) ? userNameInitialsDictionary[summaryInitials] : "Error";

               //string[] creatorInitialsArray = creator.Split('.');
               //string creatorInitials = RemoveDiacritics(creatorInitialsArray[0].Substring(0, 1) + creatorInitialsArray[1].Substring(0, 1));

               //return summaryInitials.Equals(creatorInitials, StringComparison.CurrentCultureIgnoreCase) ? creator : "Error";
            }
         }
         else
         {
            result = "REMOVED";
         }

         return result;
      }

      private static string RemoveDiacritics(string text)
      {
         return string.Concat(
            text.Normalize(NormalizationForm.FormD)
               .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) !=
                            UnicodeCategory.NonSpacingMark)
         ).Normalize(NormalizationForm.FormC);
      }
   }
}
