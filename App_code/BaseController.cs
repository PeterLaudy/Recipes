using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Recepten.Models.DB;

namespace Recepten
{
    public abstract class BaseController : Controller
    {
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
            return await userManager.FindByNameAsync(User.Identity.Name);
        }

        protected JsonResult ResultOK()
        {
            return Json(new { status = "OK" });
        }

        protected JsonResult ResultNOK(string why)
        {
            return Json(new { status = "NOK", reason = why });
        }
    }
}