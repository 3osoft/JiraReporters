namespace JiraReporter.Reporters.Writer
{
   public class ReportWriter<T>
   {
      private readonly BaseReporter<T> _reporter;
      private readonly IWritable<T> _writable;

      public ReportWriter(BaseReporter<T> reporter, IWritable<T> writable)
      {
         _reporter = reporter;
         _writable = writable;
      }

      public void Write()
      {
         _writable.Write(_reporter.Report());
      }
   }
}