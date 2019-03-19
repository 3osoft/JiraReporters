using JiraReporter.Configuration;
using JiraReporter.Reporters.Writer;

namespace JiraReporter.Gmail
{
   public abstract class WritableGmail<T> : Gmail, IWritable<T>
   {
      protected WritableGmail(GmailSettings settings) : base(settings)
      {
      }

      public abstract void Write(T data);
   }
}