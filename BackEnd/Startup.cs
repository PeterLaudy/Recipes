using System;
using System.Configuration;
using System.Linq;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Recepten.Models.DB;
using Recepten.Services;

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
            // TODO: Check if we can remove this.
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(Configuration =>
            {
                Configuration.RootPath = "wwwroot";
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

            if (!string.IsNullOrEmpty(Configuration.GetValue("PathBase", string.Empty)))
            {
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.All;
                });
            }

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

            // The lockout options are the default value, but it shows how to change them.
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout = new()
                {
                    AllowedForNewUsers = true,
                    DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5),
                    MaxFailedAccessAttempts = 5
                };
            })
            .AddTokenProvider("JWT", typeof(AuthenticationService))
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<Context>();

            services.AddAuthorization();
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Custom scheme name
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetValue("JWT:JWTKey", string.Empty))),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                };
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
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, MyAuthorizationMiddleware>();
            services.AddSingleton<AuthenticationService>();
            services.AddScoped<CheckForFirstRegistration>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration,
            ILogger<Startup> logger,
            Context context)
        {
            if (!string.IsNullOrEmpty(configuration.GetValue("PathBase", string.Empty)))
            {
                app.UseForwardedHeaders();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // Enabled when using HTTPS redirect
                if (configuration.GetValue<bool>("RedirectHTTPS", false))
                {
                    app.UseHsts();
                }
            }

            // Enabled when using HTTPS redirect
            if (configuration.GetValue<bool>("RedirectHTTPS", false))
            {
                app.UseHttpsRedirection();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            var pathBase = configuration.GetValue("PathBase", string.Empty);
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            app.UseRouting();

            // Required when the server runs behind a reversed proxy.
            if (!string.IsNullOrEmpty(configuration.GetValue("AllowedOrigins", string.Empty)))
            {
                app.UseCors("allowedSpecificOrigins");
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<CheckForFirstRegistration>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpaStaticFiles();

/*
            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = @"..\FrontEnd";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer("start");
                }
            });
*/
            this.AddRoleIfNotExists(roleManager, ApplicationRole.AdminRole);
            this.AddRoleIfNotExists(roleManager, ApplicationRole.EditorRole);
            this.AddRoleIfNotExists(roleManager, ApplicationRole.EmailVerifiedRole);

            // Check if we have at least one user.
            if (0 == userManager.Users.Count())
            {
                var firstName = configuration.GetValue("FirstUser:FirstName", string.Empty);
                var lastName = configuration.GetValue("FirstUser:LastName", string.Empty);
                var userName = configuration.GetValue("FirstUser:UserName", string.Empty);
                var eMail = configuration.GetValue("FirstUser:EMail", string.Empty);
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(eMail))
                {
                    CheckForFirstRegistration.FirstUserExists = false;
                }
                else
                {
                    var newUser = new ApplicationUser()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        UserName = userName,
                        Email = eMail,
                    };

                    var result = userManager.CreateAsync(newUser, Guid.NewGuid().ToString()).Result;
                    if (result.Succeeded)
                    {
                        newUser = userManager.FindByNameAsync(userName).Result;
                        userManager.AddToRolesAsync(newUser, [ApplicationRole.AdminRole, ApplicationRole.EditorRole]).Wait();
                    }
                    else
                    {
                        CheckForFirstRegistration.FirstUserExists = false;
                    }
                }
            }
        }

        /// <summary>
        /// Create a user role for this application if it does not yet exists.
        /// </summary>
        /// <remarks>
        /// No sense in making this async, as the caller method cannot be async.
        /// </remarks>
        private void AddRoleIfNotExists(RoleManager<ApplicationRole> roleManager, string roleName)
        {
            if (!roleManager.RoleExistsAsync(roleName).Result)
            {
                roleManager.CreateAsync(new ApplicationRole() {
                    Name = roleName,
                }).Wait();
            }
        }
    }
}
