using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Recepten
{
    public class EmailAddress
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
    }

    public interface IContactsServer
    {
        Task<List<EmailAddress>> GetAllEmailAdressesAsync();
    }

    public class ContactsServer: IContactsServer
    {
        private static List<EmailAddress> AddressListCache = new();
        private static DateTime LastCacheUpdate = DateTime.Now;

        private TimeSpan interval;

        private GoogleCredential credential;

        private PeopleServiceService peopleService;

        public ContactsServer(IWebHostEnvironment environment, IConfiguration configuration)
        {
            interval = TimeSpan.FromMinutes(configuration.GetValue("MailAddressesRefreshMinutes", 0));

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

        public async Task<List<EmailAddress>> GetAllEmailAdressesAsync()
        {
            // If we have a list of addresses...
            if (AddressListCache.Count > 0)
            {
                // ...and the interval is infinite (-1) or has not yet expired...
                if ((interval < TimeSpan.Zero) || (LastCacheUpdate.Add(interval) > DateTime.Now))
                {
                    // ...we return the list we have.
                    return AddressListCache;
                }
            }

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

                LastCacheUpdate = DateTime.Now;
                AddressListCache = result.OrderBy(email => email.FirstName).ToList();
                return AddressListCache;
            });
        }
    }
}