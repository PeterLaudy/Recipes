using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Recepten.Models.DB;
using Recepten.Services;

namespace Recepten.Controllers
{
    public class AdminController : BaseController
    {
        private static Dictionary<int, string> tokens = [];
        private int adminTokenKey;

        public AdminController(
            ILogger<DataController> logger,
            Context context,
            UserManager<ApplicationUser> userManager,
            AuthenticationService authenticationManager,
            IConfiguration configuration)
            : base(logger, context, userManager, authenticationManager)
        {
            adminTokenKey = configuration.GetValue("TokenKey", 0);
        }

        [HttpGet]
        public JsonResult GetToken(string purpose)
        {
            if (string.IsNullOrEmpty(purpose))
            {
                return ResultOK();     
            }

            var challenge = (new Random((int)DateTime.Now.Ticks)).Next();
            tokens.Add(challenge, purpose);
            return Json(challenge);
        }

        [HttpGet]
        public JsonResult ExecuteToken(string purpose, int response)
        {
            var challenge = response ^ adminTokenKey;
            if (tokens.ContainsKey(challenge) && purpose == tokens[challenge])
            {
                tokens.Remove(challenge);
                switch (purpose.ToUpper())
                {
                    case "SHUTDOWN":
                        _ = Program.Stop();
                        break;
                }
            }

            return ResultOK();
        }
    }
}