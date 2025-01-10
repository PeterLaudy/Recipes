using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Recepten.Models.DB;

namespace Recepten
{
    public abstract class BaseController : Controller
    {
        public static string RESULT_OK = "OK";
        public static string RESULT_NOK = "NOK";
        public static string RESULT_MAIL_CONFIRMATION_FAILED = "ERR__MAIL_CONFIRMATION_FAILED";
        public static string RESULT_LOGIN_FAILED = "ERR_LOGIN_FAILED";
        public static string RESULT_ACCOUNT_BLOCKED = "ERR_ACCOUNT_BLOCKED";
        public static string RESULT_MAIL_ALREADY_CONFIRMED = "ERR_MAIL_ALREADY_CONFIRMED";
        public static string RESULT_PASSWORD_UNEQUAL = "ERR_PASSWORD_UNEQUAL";
        public static string RESULT_PASSWORD_INVALID = "ERR_PASSWORD_INVALID";

        protected readonly Context context;
        protected readonly ILogger<BaseController> logger;
        protected readonly UserManager<ApplicationUser> userManager;
        protected readonly AuthenticationService authenticationManager;

        protected BaseController(
            ILogger<BaseController> logger,
            Context context,
            UserManager<ApplicationUser> userManager,
            AuthenticationService authenticationManager)
        {
            this.context = context;
            this.logger = logger;
            this.userManager = userManager;
            this.authenticationManager = authenticationManager;
        }

        protected async Task<ApplicationUser> GetCurrentUser()
        {
            return await this.userManager.FindByNameAsync(User.Identity.Name);
        }
    }
}