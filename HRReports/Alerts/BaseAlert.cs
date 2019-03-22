namespace HRReports.Alerts
{
   public abstract class BaseAlert
   {
      protected abstract string AlertText { get; }
      public abstract string ToMailBody();
   }
}