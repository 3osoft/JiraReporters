using System;
using System.IO;
using System.Net.Mail;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;

namespace JiraReporterCore.Gmail
{
   public class GmailClient
   {
      static string[] Scopes = { GmailService.Scope.GmailSend };
      private const string ApplicationName = "Gmail API .NET Quickstart";
      private const string CredentialsPath = "credentials_gmail.json";
      private const string CredPath = "token_gmail.json";

      private static GmailService _service;
      private static GmailService Service => _service ?? (_service = Initialize());

      private static GmailService Initialize()
      {
         UserCredential credential;
         using (var stream =
            new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read))
         {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
               GoogleClientSecrets.Load(stream).Secrets,
               Scopes,
               "user",
               CancellationToken.None,
               new FileDataStore(CredPath, true)).Result;
         }
         
         var service = new GmailService(new BaseClientService.Initializer
         {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
         });

         return service;
      }

      public void SendMail(MailMessage mail)
      {
         var mimeMessage = MimeMessage.CreateFromMailMessage(mail);

         var result = Service.Users.Messages.Send(new Message
         {
            Raw = Base64UrlEncode(mimeMessage.ToString())
         }, mail.From.Address).Execute();
      }

      private static string Base64UrlEncode(string input)
      {
         var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
         // Special "url-safe" base64 encode.
         return Convert.ToBase64String(inputBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
      }
   }
}