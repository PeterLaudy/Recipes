using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Recepten.Models.DB;

namespace Recepten
{
    public class AuthenticationService : IUserTwoFactorTokenProvider<ApplicationUser>
    {
        private ILogger<AuthenticationService> logger;
        private SymmetricSecurityKey securityKey;

        public AuthenticationService(
            ILogger<AuthenticationService> logger,
            IConfiguration configuration)
        {
            this.logger = logger;
            securityKey = new(Encoding.ASCII.GetBytes(configuration.GetValue("JWTKey", string.Empty)));
        }

        /// <summary>
        /// Return the claims for the given user.
        /// It contains the username and all roles for this user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<ClaimsIdentity> GenerateClaims(string purpose, UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            var claims = new ClaimsIdentity();

            claims.AddClaim(new Claim(ClaimTypes.UserData, purpose));
            claims.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

            (await userManager.GetRolesAsync(user)).ToList().ForEach(role =>
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, role));
            });

            return claims;
        }

        public async Task<string> CreateToken(string purpose, UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = await GenerateClaims(purpose, userManager, user),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = credentials
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        #region IUserTwoFactorTokenProvider methods

        public async Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return await CreateToken(purpose, manager, user);
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    var validationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = securityKey,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero
                    });

                    result = validationResult.IsValid && validationResult.Claims[ClaimTypes.UserData].Equals(purpose);
                    logger.LogWarning($"Validating token {purpose} = {validationResult.Claims[ClaimTypes.UserData]} => {result}");
                }
                catch (Exception e)
                {
                    logger.LogError($"{e.Message}");
                }
            }

            return result;
         }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return Task.FromResult(false);
        }

        #endregion IUserTwoFactorTokenProvider methods
    }
}