using JiraReporterCore.Configuration;
using JiraReporterCore.Reporters.Writer;

namespace JiraReporterCore.Gmail
{
   public abstract class WritableGmail<T> : Gmail, IWritable<T>
   {
      protected WritableGmail(GmailSettings settings) : base(settings)
      {
      }

      public abstract void Write(T data);
   }
}