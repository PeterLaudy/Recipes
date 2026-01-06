using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Recepten.Models.DB;

namespace Recepten
{
    public class Program
    {
        private static WebApplication webApp;

        internal static Task Stop()
        {
            if (null != webApp)
            {
                return Task.Delay(100).WaitAsync(CancellationToken.None).ContinueWith((_) => { webApp.StopAsync(); });
            }

            return Task.CompletedTask;
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Environment.ContentRootPath = Directory.GetCurrentDirectory();

            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services);

            webApp = builder.Build();
            webApp.Urls.Add("http://0.0.0.0:8080");

            var services = webApp.Services.CreateScope();

            startup.Configure(
                webApp, webApp.Environment,
                services.ServiceProvider.GetService<UserManager<ApplicationUser>>()!,
                services.ServiceProvider.GetService<RoleManager<ApplicationRole>>()!,
                services.ServiceProvider.GetService<IConfiguration>()!,
                services.ServiceProvider.GetService<Context>()!);

            webApp.Run();
        }

        /// <summary>
        /// This is used by dotnet-ef for database migrations etc.
        /// </summary>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
