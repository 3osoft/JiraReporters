﻿using JiraReporter.Configuration;
using JiraReporter.Reporters.Writer;

namespace JiraReporter.GSheets
{
   public abstract class WritableGoogleSheet<T> : GoogleSheet, IWritable<T>
   {
      protected WritableGoogleSheet(GoogleSheetsSettings settings) : base(settings)
      {
      }

      public abstract void Write(T dataToWrite);
   }
}