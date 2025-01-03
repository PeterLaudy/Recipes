using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.PeopleService.v1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Google.Apis.PeopleService.v1.Data;

namespace zestien3.carddav
{
    public class ContactsServer
    {
        public class EmailAddress
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
        }

        private GoogleCredential credential;

        private PeopleServiceService peopleService;

        public ContactsServer(IWebHostEnvironment environment)
        {
            // To add a scope to a service account, visit https://admin.google.com/ac/owl/domainwidedelegation?hl=en_GB
            // or on the Google Workspace Admin console, select "Security > Access and data control > API controls > Domain-wide delegation" from the menu
            credential = GoogleCredential
                .FromStream(new FileStream(Path.Combine(environment.ContentRootPath, "google-key-file.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
                .CreateScoped(new string[] { PeopleServiceService.Scope.ContactsReadonly })
                .CreateWithUser("info@zestien3.nl");

            peopleService = new PeopleServiceService( new BaseClientService.Initializer {
                HttpClientInitializer = credential,
                ApplicationName = "Recepten"
            });
        }

        public async Task<List<EmailAddress>> GetAllEmailAdressesAsync(IConfiguration configuration)
        {
            return await Task<List<EmailAddress>>.Run(() =>
            {
                var result = new List<EmailAddress>();
                var list = peopleService.People.Connections.List("people/me");
                list.PersonFields = "names,emailAddresses";
                ListConnectionsResponse response = null;
                do
                {
                    response = list.Execute();
                    list.PageToken = response.NextPageToken;
                    var allContacts = response.Connections.ToList();

                    allContacts.ForEach(contact =>
                    {
                        if (contact.EmailAddresses != null)
                        {
                            contact.EmailAddresses.ToList().ForEach(mail =>
                            {
                                var email = new EmailAddress()
                                {
                                    Address = mail.Value,
                                    FirstName = contact.Names[0].GivenName,
                                    LastName = contact.Names[0].FamilyName
                                };
                                result.Add(email);
                            });
                        }
                    });
                } while (!string.IsNullOrEmpty(response.NextPageToken));
                return result.OrderBy(email => email.FirstName).ToList();
            });
        }
    }
}