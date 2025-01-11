using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Recepten.Models.Account;
using Recepten.Models.DB;
using System.IO;
using System.Security.Claims;

namespace Recepten.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AccountController : BaseController
    {
        #region Data and Constructor

        private readonly IWebHostEnvironment environment;
        private readonly IMyEmailSender emailSender;

        public AccountController(
            ILogger<AccountController> logger,
            Context context,
            UserManager<ApplicationUser> userManager,
            AuthenticationService authenticationManager,
            IWebHostEnvironment environment,
            IMyEmailSender emailSender)
            : base(logger, context, userManager, authenticationManager)
        {
            this.environment = environment;
            this.emailSender = emailSender;
        }

        #endregion Data and Constructor

        #region Internal helper methods

        private string ValidatePasswordStrength(PasswordOptions options, string password)
        {
            bool isValid = true;
            if (options.RequireNonAlphanumeric)
            {
                if (password.All(x => (x >= 'A' && x <= 'Z') || (x >= 'a' && x <= 'z') || (x >= '0' && x <= '9')))
                {
                    isValid = false;
                }
            }

            if (options.RequireDigit)
            {
                if (password.All(x => x < '0' || x > '9'))
                {
                    isValid = false;
                }
            }

            isValid = isValid &&
                (options.RequiredLength <= password.Length) &&
                (!options.RequireUppercase || !password.ToLower().Equals(password)) &&
                (!options.RequireLowercase || !password.ToUpper().Equals(password)) &&
                (options.RequiredUniqueChars <= password.Distinct().Count());

            return isValid ? "" : "Wachtwoord moet minstens bestaan uit " +
                (options.RequireDigit ? "1 cijfer en " : "") +
                (options.RequireLowercase ? "1 kleine letter en " : "") +
                (options.RequireUppercase ? "1 hoofdletter en " : "") +
                (options.RequireNonAlphanumeric ? "1 speciaal karakter en " : "") +
                $"minimaal {options.RequiredLength} karakters lang zijn " +
                ((options.RequiredUniqueChars > 1) ? 
                    $" en minstens {options.RequiredUniqueChars} verschillende karakters bevatten" : "");
        }

        /// <summary>
        /// Add the authentication token to the request header
        /// </summary>
        /// <remarks>
        /// If token is null or empty, we tell Angular we logged out.
        /// </remarks>
        /// <param name="token">The token to add to the header</param>
        private void AddAuthenticationTokenToRequest(string token = null)
        {
            // The middleware can attach the authentication header with value Register First.
            // This to tell angular we first need to visit the registration page.
            if (!this.HttpContext.Response.Headers.ContainsKey("Authorization") ||
                this.HttpContext.Response.Headers["Authorization"] == "Bearer")
            {
                // If the token is null, we log out the user.
                // This will require some help from the angular application,
                // as we need it to no longer send the JWT Bearer token.
                if (string.IsNullOrEmpty(token))
                {
                    token = "Bearer";
                }
                else
                {
                    token = $"Bearer {token}";
                }

                this.HttpContext.Response.Headers["authorization"] = token;
            }
        }

        private void LogoutTheCurrectUser()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                AddAuthenticationTokenToRequest();
            }
        }

        #endregion Internal helper methods

        #region Basic API: Login, Logout and IsAuthenticated

        [HttpGet]
        [Route("/api/isauthenticated")]
        public JsonResult IsAuthenticated()
        {
            return Json(this.User.Identity.IsAuthenticated ? RESULT_OK : RESULT_NOK);
        }

        [HttpPost]
        [Route("/api/login")]
        public async Task<JsonResult> Login([FromBody] LoginModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                logger.LogInformation($"User {this.User.Identity.Name} was signed out because of new login attempt.");
                AddAuthenticationTokenToRequest();
            }

            if (this.ModelState.IsValid)
            {
                // Check if we can find the user in our database.
                var user = this.userManager.Users.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                if (null == user)
                {
                    return Json(RESULT_NOK);
                }

                // Check if the password was correct.
                if (this.userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
                {
                    return Json(RESULT_NOK);
                }

                AddAuthenticationTokenToRequest(await this.authenticationManager.CreateToken("login", userManager, user));
                this.context.SaveChanges();

                return Json(RESULT_OK);
            }

            return Json(RESULT_NOK);
        }

        [HttpGet]
        [Route("/api/logout")]
        public JsonResult Logout()
        {
            AddAuthenticationTokenToRequest();
            return Json(this.User.Identity.IsAuthenticated ? RESULT_NOK : RESULT_OK);
        }

        #endregion Basic API: Login, Logout and IsAuthenticated

        #region All code for registering new user and verifying their email address

        /// <summary>
        /// Create a new user and add it to the databse.
        /// </summary>
        /// <remarks>
        /// If this is the first user which is added, it will get superpower privileges.
        /// </remarks>
        /// <param name="model">The info about this user as received from the frontend.</param>
        /// <returns>The newly generated user or null if the call failed.</returns>
        private async Task<ApplicationUser> AddNewUserAsync(RegisterUserModel model)
        {
            if (null == await this.userManager.FindByNameAsync(model.UserName))
            {
                var newUser = new ApplicationUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Email = model.EMailAddress
                };

                var result = await this.userManager.CreateAsync(newUser, Guid.NewGuid().ToString());
                if (result.Succeeded)
                {
                    CheckForFirstRegistration.FirstUserExists = true;
                    newUser = await this.userManager.FindByNameAsync(model.UserName);
                    if (1 == userManager.Users.Count())
                    {
                        await userManager.AddToRolesAsync(newUser, [ApplicationRole.AdminRole, ApplicationRole.EditorRole]);
                    }

                    return newUser;
                }
            }

            return null;
        }

        /// <summary>
        /// Send the user an email to verify their email address.
        /// </summary>
        /// <param name="user">The user to send the mail to.</param>
        private async Task SendEmailVerificationMail(ApplicationUser user)
        {
            var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);

            string fileName = Path.Combine(this.environment.ContentRootPath, "verificationmail.html");
            string content = System.IO.File.ReadAllText(fileName);
            content = content
                .Replace("<USERNAME>", user.UserName)
                .Replace("<TOKEN>", Convert.ToBase64String(Encoding.ASCII.GetBytes(token)));
            await this.emailSender.SendEmailAsync(user.Email, "Email verificatie voor recepten web-site", content);
        }

        [HttpPost]
        [Route("/api/register-user")]
        /// <summary>
        /// We add a new user to the database. This can only be done by somebody with an admin role.
        /// Unless we have no users at all, then the first user to sign up will become admin.
        /// </summary>
        public async Task<JsonResult> RegisterUser([FromBody] RegisterUserModel model)
        {
            logger.LogInformation($"Registering new user.");

            // The first user exists, so we need to authenticate the current user
            // and make sure they have an admin role. 
            if (CheckForFirstRegistration.FirstUserExists)
            {
                // If no user is authenticated, we exit.
                if (!this.User.Identity.IsAuthenticated)
                    return Json(RESULT_NOK);
                var user = await this.userManager.FindByNameAsync(this.User.Identity.Name);
                // If no user is found, we exit. Probably impossible with an authenticaed user.
                if (null == user)
                    return Json(RESULT_NOK);
                // If the user is not an admin, we exit.
                if (!(await this.userManager.GetRolesAsync(user)).Contains(ApplicationRole.AdminRole))
                    return Json(RESULT_NOK);
            }

            if (ModelState.IsValid)
            {
                // We will send it to them to verify their mail address.
                var user = await this.AddNewUserAsync(model);
                if (null != user)
                {
                    await SendEmailVerificationMail(user);
                    return Json(RESULT_OK);
                }
            }

            return Json(RESULT_NOK);
        }

        [HttpGet]
        [Route("/api/verify-email")]
        /// <summary>
        /// The user clicked the email verification link they received.
        /// The token in the link is send to us by the frontend.
        /// </summary>
        public async Task<JsonResult> VerifyEmail(string token)
        {
            logger.LogInformation("Verifying email address.");

            var jsonToken = await authenticationManager.DecodeTokenAsync(token);
            if (jsonToken.IsValid)
            {
                // The claims in the token will provide us the username.
                var user = await this.userManager.FindByNameAsync(jsonToken.Claims[ClaimTypes.Name] as string);
                if (null != user)
                {
                    // Check if the token is OK.
                    if ((await this.userManager.ConfirmEmailAsync(user, token)).Succeeded)
                    {
                        // We need to add the EmailVerified role for this user.
                        var confirmResult = await userManager.AddToRoleAsync(user, ApplicationRole.EmailVerifiedRole);
                        if (confirmResult.Succeeded)
                        {
                            // Now we need to reset the password of the user and send them the token.
                            var pswdToken = await this.userManager.GeneratePasswordResetTokenAsync(user);
                            // The token noe goes to the frontend, where the user will add the password (2x)
                            // and send that back again. Then we can follow the normal route.
                            return Json(pswdToken);
                        }
                        else
                        {
                            // TODO: An admin has to send a new link to the user, as this one did not work.
                            // No need to do enything here. We need a kind of admin page where we can do these things.
                        }
                    }
                }
            }

            return Json(RESULT_NOK);
        }

        #endregion All code for registering new user and verifying their emaiol address

        #region All code for changing the password

        [HttpPost]
        [Route("/api/request-password-change")]
        public async Task<IActionResult> RequestPasswordChange(string userName)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                AddAuthenticationTokenToRequest();
            }

            var user = await this.userManager.FindByNameAsync(userName);
            if (null != user)
            {
                logger.LogInformation($"Password reset for user {userName} requested.");
                // Send the user a link to reset the password.
                // This is the code used by TimeTracker. We should use the Google mail sender.
                var token = await this.userManager.GeneratePasswordResetTokenAsync(user);
                string fileName = Path.Combine(this.environment.ContentRootPath, "passwordresetmail.html");
                await this.emailSender.SendEmailAsync(
                    user.Email,
                    "Password reset",
                    System.IO.File.ReadAllText(fileName).Replace("<TOKEN>", Convert.ToBase64String(Encoding.ASCII.GetBytes(token)))
                );
            }

            // Always send OK, even if no mail has been send.
            // We don't want people to try which usernames are in use.
            return Json(RESULT_OK);
        }

        [HttpPost]
        [Route("/api/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                AddAuthenticationTokenToRequest();
            }

            if (ModelState.IsValid)
            {
                logger.LogInformation("Changing password.");
                var jsonToken = await this.authenticationManager.DecodeTokenAsync(model.Token);
                if (jsonToken.IsValid)
                {
                    if ((jsonToken.Claims[ClaimTypes.Name] as string) == model.UserName)
                    {
                        var user = await this.userManager.FindByNameAsync(model.UserName);
                        if (null != user)
                        {
                            string validPasswordCheck = this.ValidatePasswordStrength(this.userManager.Options.Password, model.Password);
                            if (string.IsNullOrEmpty(validPasswordCheck))
                            {
                                await this.userManager.ResetPasswordAsync(
                                    user,
                                    this.userManager.GeneratePasswordResetTokenAsync(user).Result,
                                    model.Password
                                );

                                AddAuthenticationTokenToRequest(await this.authenticationManager.CreateToken("login", userManager, user));
                                this.context.SaveChanges();
                                return Json(RESULT_OK);
                            }
                        }
                    }
                }
            }

            return Json(RESULT_NOK);
        }

        #endregion All code for changing the password
    }
}