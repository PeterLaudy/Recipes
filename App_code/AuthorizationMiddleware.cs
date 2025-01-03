using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Recepten
{

    public class AuthorizationMiddleware : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            // If the authorization was not succesfull for an API call,
            // provide a 401 (unauthorized) response instead of the standard 302 (redirect) to the login page.
            if (!authorizeResult.Succeeded && context.Request.Path.StartsWithSegments("/api"))
            {
                // Return a 401 instead of the default 302.
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}