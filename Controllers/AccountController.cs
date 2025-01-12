using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Recepten.Models.Account;
using Recepten.Models.DB;
using System.IO;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Recepten.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AccountController : BaseController
    {
        #region Data and Constructor

        private const string PURPOSE_LOGIN = "login";
        private const string PURPOSE_VERIFY_EMAIL = "verify email";
        private const string PURPOSE_CHANGE_PASSWORD = "change password";

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
        private void AddAuthenticationTokenToRequest(string token)
        {
            // The middleware can attach the authentication header with value Register First.
            // This to tell angular we first need to visit the registration page.
            // The middleware is called before us, so we are allowed to overrule the Logout.
            if (!this.HttpContext.Response.Headers.ContainsKey("Authorization") ||
                this.HttpContext.Response.Headers["Authorization"] == "Logout")
            {
                this.HttpContext.Response.Headers["authorization"] = token;
            }
        }

        private void LogoutUser()
        {
            this.AddAuthenticationTokenToRequest("Logout");
        }

        private void LogoutTheCurrectUser()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                LogoutUser();
            }
        }

        private JsonResult CheckLink(TokenValidationResult jsonToken)
        {
            if (null != jsonToken.Exception)
            {
                return ResultNOK(jsonToken.Exception.Message);
            }

            return ResultNOK("Invalid link.");
        }

        #endregion Internal helper methods

        #region Basic API: Login, Logout and IsAuthenticated

        [HttpGet]
        [Route("/api/isauthenticated")]
        public JsonResult IsAuthenticated()
        {
            return Json(new { status = "OK", isAuthenticated = this.User.Identity.IsAuthenticated });
        }

        [HttpPost]
        [Route("/api/login")]
        public async Task<JsonResult> Login([FromBody] LoginModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                logger.LogInformation($"User {this.User.Identity.Name} was signed out because of new login attempt.");
                LogoutUser();
            }

            if (this.ModelState.IsValid)
            {
                // Check if we can find the user in our database.
                var user = await this.userManager.FindByNameAsync(model.UserName);
                if (null == user)
                {
                    return ResultNOK("Incorrect username or password");
                }

                if (!user.EmailConfirmed)
                {
                    await SendEmailVerificationMail(user);
                    return ResultNOK("Bevestig eerst je email adres via de link die je in je mailbox vindt.");
                }

                // Check if the password was correct.
                if (this.userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
                {
                    return ResultNOK("Incorrect username or password");
                }

                AddAuthenticationTokenToRequest($"Bearer {await this.authenticationManager.CreateToken(PURPOSE_LOGIN, userManager, user)}");

                return ResultOK();
            }

            return ResultNOK("Incorrect username or password");
        }

        [HttpGet]
        [Route("/api/logout")]
        public JsonResult Logout()
        {
            LogoutUser();
            return ResultOK();
        }

        #endregion Basic API: Login, Logout and IsAuthenticated

        #region All code for registering new user and verifying their email address

        /// <summary>
        /// Create a new user and add it to the databse.
        /// </summary>
        /// <remarks>
        /// If this is the first user which is added, it will get superpower privileges.
        /// An error message always contains a space, a username not. It's a bit dodgy,
        /// but it works. I don't like using out parameters.
        /// </remarks>
        /// <param name="model">The info about this user as received from the frontend.</param>
        /// <returns>The username of the newly generated user or an error message if the call failed.</returns>
        private async Task<string> AddNewUserAsync(RegisterUserModel model)
        {
            if (null == await this.userManager.FindByNameAsync(model.UserName))
            {
                if (null == await this.userManager.FindByEmailAsync(model.EMailAddress))
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

                        return model.UserName;
                    }

                    return "Unable to register the user";
                }

                return "EMail address is already registered";
            }

            return "Username is already registered";
        }

        /// <summary>
        /// Send the user an email to verify their email address.
        /// </summary>
        /// <param name="user">The user to send the mail to.</param>
        private async Task SendEmailVerificationMail(ApplicationUser user)
        {
            var token = await this.authenticationManager.CreateBase64Token(PURPOSE_VERIFY_EMAIL, this.userManager, user);

            string fileName = Path.Combine(this.environment.ContentRootPath, "verificationmail.html");
            string content = System.IO.File.ReadAllText(fileName);
            content = content.Replace("<USERNAME>", user.UserName).Replace("<TOKEN>", token);
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

            if (CheckForFirstRegistration.FirstUserExists)
            {
                // The first user exists, so we need to authenticate the current user
                // and make sure they have an admin role. 

                var user = await this.GetCurrentUser();
                // If no user is found, we exit..
                if ((null == user) ||
                    // If no user is authenticated, we exit.
                    !this.User.Identity.IsAuthenticated ||
                    // If the user is not an admin, we exit.
                    !(await this.userManager.GetRolesAsync(user)).Contains(ApplicationRole.AdminRole))
                {
                    return ResultNOK("You don't have persmission to register new users.");
                }
            }

            if (ModelState.IsValid)
            {
                // We will send it to them to verify their mail address.
                var result = await this.AddNewUserAsync(model);
                if (!result.Contains(' '))
                {
                    var newUser = await this.userManager.FindByNameAsync(model.UserName);
                    await SendEmailVerificationMail(newUser);
                    return ResultOK();
                }

                return ResultNOK(result);
            }

            return ResultNOK("Check the data you entered.");
        }

        [HttpPost]
        [Route("/api/verify-email")]
        /// <summary>
        /// The user clicked the email verification link they received.
        /// The token in the link is send to us by the frontend.
        /// </summary>
        public async Task<JsonResult> VerifyEmail([FromBody] VerifyEmailModel model)
        {
            logger.LogInformation("Verifying email address.");

            if (ModelState.IsValid)
            {
                var jsonToken = await authenticationManager.DecodeBase64TokenAsync(model.Token);
                if (jsonToken.IsValid && (string)jsonToken.Claims[ClaimTypes.UserData] == PURPOSE_VERIFY_EMAIL)
                {
                    if ((jsonToken.Claims[ClaimTypes.Name] as string) == model.UserName)
                    {
                        var user = await this.userManager.FindByNameAsync(model.UserName);
                        if (null != user)
                        {
                            // We cannot use UserManager.ConfirmEmailAsync, because we use different tokens.
                            // We therefore also need to update the database ourselves.
                            user.EmailConfirmed = true;
                            this.context.Update(user);
                            this.context.SaveChanges();

                            // We need to add the EmailVerified role for this user.
                            var confirmResult = await userManager.AddToRoleAsync(user, ApplicationRole.EmailVerifiedRole);
                            if (confirmResult.Succeeded)
                            {
                                // Now we need to reset the password of the user and send them the token.
                                var pswdToken = await this.authenticationManager.CreateBase64Token(PURPOSE_CHANGE_PASSWORD, this.userManager, user);
                                // The token now goes to the frontend, where the user will add the password (2x)
                                // and send that back again. Then we can follow the normal route.
                                return Json(new { status = "OK", token = pswdToken });
                            }
                        }
                    }
                }
                else
                {
                    return CheckLink(jsonToken);
                }
            }

            return ResultNOK( "Unexpected failure." );
        }

        #endregion All code for registering new user and verifying their emaiol address

        #region All code for changing the password

        [HttpPost]
        [Route("/api/request-password-change")]
        public async Task<IActionResult> RequestPasswordChange([FromBody] LoginModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                LogoutUser();
            }

            var user = await this.userManager.FindByNameAsync(model.UserName);
            if (null != user)
            {
                logger.LogInformation($"Password reset for user {model.UserName} requested.");
                // Send the user a link to reset the password.
                // This is the code used by TimeTracker. We should use the Google mail sender.
                var token = await this.authenticationManager.CreateBase64Token(PURPOSE_CHANGE_PASSWORD, this.userManager, user);
                string fileName = Path.Combine(this.environment.ContentRootPath, "passwordresetmail.html");
                await this.emailSender.SendEmailAsync(
                    user.Email,
                    "Password reset",
                    System.IO.File.ReadAllText(fileName).Replace("<TOKEN>", token)
                );
            }

            // Always send OK, even if no mail has been send.
            // We don't want people to try which usernames are in use.
            return ResultOK();
        }

        [HttpPost]
        [Route("/api/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                LogoutUser();
            }

            if (ModelState.IsValid)
            {
                logger.LogInformation("Changing password.");
                var jsonToken = await this.authenticationManager.DecodeBase64TokenAsync(model.Token);
                if (jsonToken.IsValid && (string)jsonToken.Claims[ClaimTypes.UserData] == PURPOSE_CHANGE_PASSWORD)
                {
                    if ((jsonToken.Claims[ClaimTypes.Name] as string) == model.UserName)
                    {
                        var user = await this.userManager.FindByNameAsync(model.UserName);
                        if (null != user)
                        {
                            string validPasswordCheck = this.ValidatePasswordStrength(this.userManager.Options.Password, model.Password);
                            if (string.IsNullOrEmpty(validPasswordCheck))
                            {
                                // We cannot use UserManager.ResetPasswordAsync, because we use different tokens.
                                // We therefore also need to update the database ourselves.
                                user.PasswordHash = this.userManager.PasswordHasher.HashPassword(user, model.Password);
                                this.context.Update(user);
                                this.context.SaveChanges();

                                AddAuthenticationTokenToRequest($"Bearer {await this.authenticationManager.CreateToken(PURPOSE_LOGIN, userManager, user)}");
                                return ResultOK();
                            }

                            return ResultNOK(validPasswordCheck);
                       }
                    }
                }

                return CheckLink(jsonToken);
            }

            return ResultNOK("The passwords probably don't match");
        }

        #endregion All code for changing the password
    }
}