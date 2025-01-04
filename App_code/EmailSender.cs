using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Microsoft.AspNetCore.Hosting;

namespace Recepten
{
    public interface IMyEmailSender {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class EmailSender : IMyEmailSender
    {
        private GoogleCredential credential;

        private GmailService mailService;

        public EmailSender(IWebHostEnvironment environment)
        {
            credential = GoogleCredential
                .FromStream(new FileStream(Path.Combine(environment.ContentRootPath, "google-key-file.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
                .CreateScoped(new string[] { GmailService.Scope.GmailSend })
                .CreateWithUser("info@zestien3.nl");

            mailService = new GmailService( new BaseClientService.Initializer {
                HttpClientInitializer = credential,
                ApplicationName = "Recepten"
            });
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Message msg = new Message()
            {
                Raw = Base64UrlEncode($"To: {email}\r\nSubject: {subject}\r\nContent-Type: text/html;charset=utf-8\r\n\r\n{htmlMessage}")
            };

            try
            {
                await mailService.Users.Messages.Send(msg, "me").ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        private static string Base64UrlEncode(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(data).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}