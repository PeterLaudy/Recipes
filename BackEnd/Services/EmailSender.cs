using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Recepten.Services
{
    public interface IMyEmailSender {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class EmailSender : IMyEmailSender
    {
        private GmailService mailService;

        private string baseURL;

        public EmailSender(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.baseURL = configuration.GetValue("BaseURL", string.Empty);

            // To add a scope to a service account, visit https://admin.google.com/ac/owl/domainwidedelegation?hl=en_GB
            // or on the Google Workspace Admin console, select "Security > Access and data control > API controls > Domain-wide delegation" from the menu
            var credential = GoogleCredential
                .FromStream(new FileStream(Path.Combine(environment.ContentRootPath, "google-key-file.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
                .CreateScoped(new string[] { GmailService.Scope.GmailSend })
                .CreateWithUser("info@zestien3.nl");

            mailService = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Recipes"
            });
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Message msg = new Message()
            {
                Raw = Base64UrlEncode(
                    $"To: {email}\r\n" +
                    $"Subject: {subject}\r\n" +
                    $"Content-Type: text/html;charset=utf-8\r\n\r\n" +
                    $"{htmlMessage.Replace("<BASEURL>", this.baseURL)}")
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