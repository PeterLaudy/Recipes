using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Recepten
{
    public class CheckForFirstRegistration
    {
        private readonly RequestDelegate next;
        private readonly ILogger<CheckForFirstRegistration> logger;

        public static bool FirstUserExists { get; internal set; } = true;

        public CheckForFirstRegistration(RequestDelegate next, ILogger<CheckForFirstRegistration> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!FirstUserExists)
            {
                context.Response.Headers.Add(new("authorization", "Register First"));
            }
            else
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    context.Response.Headers.Add(new("authorization", "Logout"));
                }
            }

            await next.Invoke(context);              
        }        
    }
}