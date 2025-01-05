using System;
using System.Configuration;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Recepten.Models.DB;

namespace Recepten
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

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
                        Logger.LogInformation("Using MySQL database.");
                        options.UseMySql(Configuration.GetConnectionString("MySQL"), new MySqlServerVersion(new Version(5, 7, 33)));
                        break;
                    case "SQLite":
                        Logger.LogInformation("Using SQLite database.");
                        Logger.LogInformation(Configuration.GetConnectionString("SQLite"));
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

            // The lockout options are the default value, but it shows how to change them.
            services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout = new()
                {
                    AllowedForNewUsers = true,
                    DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5),
                    MaxFailedAccessAttempts = 5
                };
            }).AddEntityFrameworkStores<Context>().AddDefaultTokenProviders();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;

            });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.AddSingleton<IContactsServer, ContactsServer>();
            services.AddSingleton<IMyEmailSender, EmailSender>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationMiddleware>();
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
            app.UseCookiePolicy();

            app.UseRouting();

            // Required when the server runs behind a reversed proxy.
            if (!string.IsNullOrEmpty(Configuration.GetValue("AllowedOrigins", string.Empty)))
            {
                app.UseCors("allowedSpecificOrigins");
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpaStaticFiles();

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
