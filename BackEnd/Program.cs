using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Recepten
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                    {
                        // When running this in a docker image, we want to be able
                        // to change the configuration without rebuilding the image.
                        // The file to use must be the first parameter on the command line.
                        if ((args.Length >= 1) && File.Exists(args[0]))
                        {
                            configBuilder.AddJsonFile(path: args[0], optional: true, reloadOnChange: false);
                        }
                    })
                .UseStartup<Startup>()
                .UseUrls(["http://0.0.0.0:8080/"])
                .Build()
                .Run();
        }

        /// <summary>
        /// This is used by dotnet-ef for database migrations etc.
        /// </summary>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
