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
            if (this.User.Identity!.IsAuthenticated)
            {
                AddAuthenticationTokenToRequest();
            }
        }

        private async Task<ApplicationUser> AddNewUserAsync(string userName, string password, string email)
        {
            if (null == await this.userManager.FindByNameAsync(userName))
            {
                var newUser = new ApplicationUser()
                {
                    UserName = userName,
                    Email = email,
                };

                var result = await this.userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    CheckForFirstRegistration.FirstUserExists = true;
                    newUser = await this.userManager.FindByNameAsync(userName)!;
                    if (1 == userManager.Users.Count())
                    {
                        await userManager.AddToRolesAsync(newUser!, ["Admin", "Editor"]);
                    }

                    return newUser;
                }
            }

            return null;
        }

        #endregion Internal helper methods

        #region Basic API: Login, Logout and IsAuthenticated

        [HttpGet]
        [Route("/api/isauthenticated")]
        public JsonResult IsAuthenticated()
        {
            return Json(this.User.Identity!.IsAuthenticated ? RESULT_OK : RESULT_NOK);
        }

        [HttpPost]
        [Route("/api/login")]
        public async Task<JsonResult> Login([FromBody] LoginViewModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                logger.LogInformation($"User {this.User.Identity!.Name} was signed out because of new login attempt.");
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
            return Json(this.User.Identity!.IsAuthenticated ? RESULT_NOK : RESULT_OK);
        }

        #endregion Basic API: Login, Logout and IsAuthenticated

        [HttpPost]
        [Route("/api/forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] LoginViewModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                AddAuthenticationTokenToRequest();
            }

            var user = await this.userManager.FindByNameAsync(model.UserName);
            if (null != user)
            {
                logger.LogInformation($"Password reset for user {user.UserName} requested.");
                // Send the user a link to reset the password.
                // This is the code used by TimeTracker. We should use the Google mail sender.
                var token = await this.userManager.GeneratePasswordResetTokenAsync(user);
                string fileName = Path.Combine(this.environment.ContentRootPath, "wachtwoordresetmail.html");
                await this.emailSender.SendEmailAsync(
                    user.Email,
                    "Password reset",
                    System.IO.File.ReadAllText(fileName).Replace("<TOKEN>", Convert.ToBase64String(Encoding.ASCII.GetBytes(token)))
                );
            }

            return Json(RESULT_OK);
        }

        private async Task<JsonResult> RegisterFirstUser(RegisterViewModel model)
        {
            logger.LogInformation("Registering new user: there are no users registered yet.");

            // This is the first user being registered. We do not have a token for them, so we create that now.
            // We will send it to them to verify their mail address.
            var user = await this.AddNewUserAsync(model.UserName, model.Password, model.EMailAddress);
            if (null != user)
            {
                var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);

                string fileName = Path.Combine(this.environment.ContentRootPath, "verificationmail.html");
                string content = System.IO.File.ReadAllText(fileName);
                content = content
                    .Replace("<USERNAME>", user.UserName)
                    .Replace("<TOKEN>", Convert.ToBase64String(Encoding.ASCII.GetBytes(token)));
                await this.emailSender.SendEmailAsync(user.Email, "Email verificatie voor recepten web-site", content);
                return Json(RESULT_OK);
            }

            return Json(RESULT_NOK);
        }

        [HttpPost]
        [Route("/api/register")]
        /// <summary>
        /// Link that the user has received by email to sign up for an account.
        /// This should confirm the email address and allow for setting a password.
        /// The email address is confirmed, because this linked is clicked.
        /// We should give an opportunity to set the password.
        /// </summary>
        public async Task<JsonResult> Register([FromBody] RegisterViewModel model)
        {
            logger.LogInformation($"Registering new user.");

            LogoutTheCurrectUser();

            if (ModelState.IsValid)
            {
                logger.LogInformation("Registering new user: model state is valid.");
                if (0 == await this.userManager.Users.CountAsync())
                {
                    // If this is the first user we register, we need to send a mail with a link.
                    // This will only be done for the very first user.
                    // If the first user is defined in the configuration file, we will never execute this code.
                    return await RegisterFirstUser(model);
                }

                var user = await this.userManager.FindByNameAsync(model.UserName);
                if (null != user)
                {
                    logger.LogInformation($"Registering new user: found the user to register => {user.UserName}.");

                    // First we check if this is called because we need to confirm the email.
                    if (!user.EmailConfirmed)
                    {
                        logger.LogInformation($"Registering new user: EMail has not yet been confirmed.");

                        var validBase64 = Convert.TryFromBase64String(model.Token, new(new byte[model.Token.Length]), out int _);
                        var token = validBase64 ? Encoding.ASCII.GetString(Convert.FromBase64String(model.Token)) : "";
                        var result = await this.userManager.ConfirmEmailAsync(user, token);
                        this.context.SaveChanges();
                        if (!result.Succeeded)
                        {
                            // Email confirmation failed.
                            // Log the reason the confirmation failed, in case the user contacts us.
                            logger.LogInformation($"EMail for user {user.UserName} failed:");
                            foreach (var error in result.Errors)
                            {
                                logger.LogInformation($"- {error.Description}");
                            }

                            return Json(RESULT_MAIL_CONFIRMATION_FAILED);
                        }

                        // Email has been confirmed. So we can try to set the password.
                        string validPasswordCheck = this.ValidatePasswordStrength(this.userManager.Options.Password, model.Password);
                        if (string.IsNullOrEmpty(validPasswordCheck))
                        {
                            if (model.Password.Equals(model.ConfirmPassword))
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
                            else
                            {
                                return Json(RESULT_PASSWORD_UNEQUAL);
                            }
                        }
                        else
                        {
                            return Json(RESULT_PASSWORD_INVALID);
                        }
                    }
                }
            }

            logger.LogInformation("Registering new user failed because of invalid model state.");
            return Json(RESULT_NOK);
        }

        [HttpPost]
        [Route("/api/resetpassword")]
        public async Task<JsonResult> ResetPassword([FromBody] RegisterViewModel model)
        {
            // Sign out anyone who is signed in.
            if (this.User.Identity!.IsAuthenticated)
            {
                logger.LogInformation($"User {this.User.Identity.Name} was signed out because of new reset attempt.");
                AddAuthenticationTokenToRequest();
            }
            
            var user = await this.userManager.FindByNameAsync(model.UserName);
            if (null != user)
            {
                // Email has been confirmed. So we can try to set the password.
                string validPasswordCheck = this.ValidatePasswordStrength(this.userManager.Options.Password, model.Password);
                if (string.IsNullOrEmpty(validPasswordCheck))
                {
                    if (model.Password.Equals(model.ConfirmPassword))
                    {
                        await this.userManager.ResetPasswordAsync(
                            user,
                            await this.userManager.GeneratePasswordResetTokenAsync(user),
                            model.Password
                        );
                        this.context.SaveChanges();

                        AddAuthenticationTokenToRequest(await this.authenticationManager.CreateToken("login", userManager, user));
                        return Json(RESULT_OK);
                    }
                    else
                    {
                        return Json(RESULT_PASSWORD_UNEQUAL);
                    }
                }
                else
                {
                    return Json(RESULT_PASSWORD_INVALID);
                }
            }
 
            return Json(RESULT_NOK);
        }
    }
}