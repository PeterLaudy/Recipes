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

namespace Recepten.Controllers
{
    public class AccountController : BaseController
    {
        private IWebHostEnvironment environment;
        private EmailSender emailSender;

        public AccountController(
            ILogger<AccountController> logger,
            Context context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
            IMyEmailSender emailSender)
            : base(logger, context, userManager, signInManager)
        {
            this.environment = environment;
            this.emailSender = emailSender as EmailSender;
        }

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

        private async Task<string> AddNewUserAsync(string userName, string password, string email)
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
                    newUser = await this.userManager.FindByNameAsync(userName);
                    var token = await this.userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    return token;
                }
            }

            return string.Empty;
        }

        [HttpGet]
        [Route("/api/isauthenticated")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public JsonResult IsAuthenticated()
        {
            logger.LogInformation(this.User.Identity.Name);
            return Json(this.User.Identity.IsAuthenticated ? RESULT_OK : RESULT_NOK);
        }

        [HttpPost]
        [Route("/api/login")]
        public async Task<JsonResult> Login([FromBody] LoginViewModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                logger.LogInformation($"User {this.User.Identity.Name} was signed out because of new login attempt.");
                await this.signInManager.SignOutAsync();
            }

            if (this.ModelState.IsValid)
            {
                // <see cref="Startup.ConfigureServices">for how to set the account lockout options.</see>
                var signinResult = await this.signInManager.PasswordSignInAsync(model.UserName, model.Password, true, true);
                if (!signinResult.Succeeded)
                {
                    if (signinResult.IsLockedOut)
                    {
                        return Json(RESULT_ACCOUNT_BLOCKED);
                    }
                    else
                    {
                        return Json(RESULT_LOGIN_FAILED);
                    }
                }

                this.context.SaveChanges();
                return Json(RESULT_OK);
            }

            return Json(RESULT_NOK);
        }

        [HttpPost]
        [Route("/api/forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] LoginViewModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                await this.signInManager.SignOutAsync();
            }

            var user = await this.userManager.FindByNameAsync(model.UserName);
            if (null != user)
            {
                logger.LogInformation($"Password reset for user {user.UserName} requested.");
                // Send the user a link to reset the password.
                // This is the code used by TimeTracker. We should use the Google mail sender.
                /*
                var token = await this.userManager.GeneratePasswordResetTokenAsync(user);
                string fileName = Path.Combine(this.environment.ContentRootPath, "wachtwoordresetmail.html");
                await this.emailSender.SendEmailAsync(
                    user.Email,
                    "Password reset",
                    System.IO.File.ReadAllText(fileName).Replace("<TOKEN>", Convert.ToBase64String(Encoding.ASCII.GetBytes(token)))
                );
                */
            }

            return Json(RESULT_OK);
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

            // Sign out anyone who is signed in.
            if (this.User.Identity.IsAuthenticated)
            {
                logger.LogInformation($"User {this.User.Identity.Name} was signed out because of new register attempt.");
                await this.signInManager.SignOutAsync();
            }

            if (ModelState.IsValid)
            {
                logger.LogInformation("Registering new user: model state is valid.");
                if (0 == await this.userManager.Users.CountAsync())
                {
                    logger.LogInformation("Registering new user: there are no users registered yet.");

                    // This is the first user being registered. We do not have a token for them, so we create that now.
                    // We set it to the model, so we are sure the email address will be verified. For the first user, tyhis is acceptable.
                    // If the user exists, AddNewUser will return an empty string and the registration will fail.
                    var token = await this.AddNewUserAsync(model.UserName, model.Password, model.EMailAddress);
                    model.Token = Convert.ToBase64String(Encoding.ASCII.GetBytes(token));
                }

                var user = await this.userManager.FindByNameAsync(model.UserName);
                if (null != user)
                {
                    logger.LogInformation($"Registering new user: found the user to register => {user.UserName}.");

                    // First we check if this is called because we need to confirm the email.
                    if (!user.EmailConfirmed)
                    {
                        logger.LogInformation($"Registering new user: EMail has not yet been confirmed.");

                        var token = Encoding.ASCII.GetString(Convert.FromBase64String(model.Token));
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
                                this.context.SaveChanges();

                                var signinResult = await this.signInManager.PasswordSignInAsync(model.UserName, model.Password, true, true);
                                if (!signinResult.Succeeded)
                                {
                                    if (signinResult.IsLockedOut)
                                    {
                                        return Json(RESULT_ACCOUNT_BLOCKED);
                                    }
                                    else
                                    {
                                        return Json(RESULT_LOGIN_FAILED);
                                    }
                                }

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
            if (this.User.Identity.IsAuthenticated)
            {
                logger.LogInformation($"User {this.User.Identity.Name} was signed out because of new reset attempt.");
                await this.signInManager.SignOutAsync();
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

                        var signinResult = await this.signInManager.PasswordSignInAsync(model.UserName, model.Password, true, true);
                        if (!signinResult.Succeeded)
                        {
                            if (signinResult.IsLockedOut)
                            {
                                return Json(RESULT_ACCOUNT_BLOCKED);
                            }
                            else
                            {
                                return Json(RESULT_LOGIN_FAILED);
                            }
                        }

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
 
            return Json(RESULT_NOK);
        }
    }
}