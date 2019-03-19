namespace JiraReporter.Reporters.Writer
{
   public interface IWritable<in T>
   {
      void Write(T data);
   }
}