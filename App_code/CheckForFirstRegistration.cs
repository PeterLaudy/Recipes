using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Recepten
{
    public class CheckForFirstRegistration
    {
        private readonly RequestDelegate _next;

        public static bool FirstUserExists { get; internal set; } = true;

        public CheckForFirstRegistration(RequestDelegate next)
        {
            _next = next;
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
                    context.Response.Headers.Add(new("authorization", "Bearer"));
                }
            }

            await _next.Invoke(context);              
        }        
    }
}