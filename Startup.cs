using System;
using System.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Recepten.Models.DB;

namespace Recepten
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });

            services.AddDbContext<Context>(options =>
            {
                switch (Configuration.GetValue("UsedDB", "SQLite"))
                {
                    case "MySQL":
                        options.UseMySql(Configuration.GetConnectionString("MySQL"), new MySqlServerVersion(new Version(5, 7, 33)));
                        break;
                    case "SQLite":
                        options.UseSqlite($"Data Source={Configuration.GetConnectionString("SQLite")}");
                        break;
                    default:
                        throw new SettingsPropertyWrongTypeException("Configuration UsedDB has an illegal value or is undefined.");
                }
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot";
            });

            // Required when the server runs behind a reversed proxy.
            // Allowed origins are defined in the appsettings.json file as an array of strings delimited 
            var allowedOrigins = Configuration.GetValue("AllowedOrigins", string.Empty);
            if (!string.IsNullOrEmpty(allowedOrigins))
            {
                var origins = allowedOrigins.Split(';');
                services.AddCors(options =>
                {
                    options.AddPolicy(name: "allowedSpecificOrigins",
                                      policy =>
                                      {
                                          policy.WithOrigins(origins);
                                      });
                });
            }

            services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Context context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // Enabled when using HTTPS redirect
                if (Configuration.GetValue<bool>("RedirectHTTPS", false))
                {
                    app.UseHsts();
                }
            }

            // Enabled when using HTTPS redirect
            if (Configuration.GetValue<bool>("RedirectHTTPS", false))
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            // Required when the server runs behind a reversed proxy.
            if (!string.IsNullOrEmpty(Configuration.GetValue("AllowedOrigins", string.Empty)))
            {
                app.UseCors("allowedSpecificOrigins");
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer("start");
                }
            });

            _ = context.SaveChanges();
        }
    }
}
