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
        private IConfiguration configuration;
        private SymmetricSecurityKey securityKey;

        public AuthenticationService(
            ILogger<AuthenticationService> logger,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
            securityKey = new(Encoding.ASCII.GetBytes(configuration.GetValue("JWT:JWTKey", string.Empty)));
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
                Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>($"JWT:TokenLifespan:{purpose}", 5)),
                SigningCredentials = credentials
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        public async Task<string> CreateBase64Token(string purpose, UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(await CreateToken(purpose, userManager, user)));
        }

        public async Task<TokenValidationResult> DecodeTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                return await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                });
            }
            catch (Exception e)
            {
                logger.LogError($"{e.Message}");
            }

            return null;
        }

        public async Task<TokenValidationResult> DecodeBase64TokenAsync(string token)
        {
            return await DecodeTokenAsync(Encoding.ASCII.GetString(Convert.FromBase64String(token.Replace("%3D", "="))));
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
                var validatedToken = await DecodeTokenAsync(token);
                result = validatedToken.IsValid && validatedToken.Claims[ClaimTypes.UserData].Equals(purpose);
                logger.LogWarning($"Validating token {purpose} = {validatedToken.Claims[ClaimTypes.UserData]} => {result}");
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