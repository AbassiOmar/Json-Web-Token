using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Secutity.IssuerJWT.Configurations;
using Secutity.IssuerJWT.Extensions;
using Secutity.IssuerJWT.Models;

namespace Secutity.IssuerJWT.Controllers
{
    [AllowAnonymous]
    [Route("api/security")]
    public class JwtIssuerController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IJwtIssuerOptions jwtOptions;
        private readonly RoleManager<IdentityRole> roleManager;

        public JwtIssuerController(IJwtIssuerOptions jwtOptions,
              UserManager<ApplicationUser> userManager,
              SignInManager<ApplicationUser> signInManager,
              RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtOptions = jwtOptions;
            this.roleManager = roleManager;
        }
        [HttpPost(nameof(Login))]
        public async Task<IActionResult> Login([FromBody] LoginViewModel login)
        {
            if (login == null)
                return BadRequest("Login must be assigned");
            var user = await userManager.FindByEmailAsync(login.Email);
            if (user == null || (!(await signInManager.PasswordSignInAsync(user, login.Password, false, false)).Succeeded))
                return BadRequest("Invalid credentials");
            var token = await CreateJWTTokenAsync(user);
            await signInManager.SignOutAsync();
            var result = new ContentResult() { Content = token, ContentType = "application/text" };
            return result;
        }

        private async Task<string> CreateJWTTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>(new[]
            {
                // Issuer
                new Claim(JwtRegisteredClaimNames.Iss, jwtOptions.Issuer),   

                // UserName
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),       

                // Email is unique
                new Claim(JwtRegisteredClaimNames.Email, user.Email),        

                // Unique Id for all Jwt tokes
                new Claim(JwtRegisteredClaimNames.Jti, await jwtOptions.JtiGenerator()),

                // Issued at
                new Claim(JwtRegisteredClaimNames.Iat, jwtOptions.IssuedAt.ToUnixEpochDate().ToString(), ClaimValueTypes.Integer64)
            });

            claims.AddRange(await userManager.GetClaimsAsync(user));
            var roleNames = await userManager.GetRolesAsync(user);
            foreach (var roleName in roleNames)
            {
                // Find IdentityRole by name
                var role = await roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    // Convert Identity to claim and add 
                    var roleClaim = new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, jwtOptions.Issuer);
                    claims.Add(roleClaim);

                    // Add claims belonging to the role
                    var roleClaims = await roleManager.GetClaimsAsync(role);
                    claims.AddRange(roleClaims);
                }
            }

            // Prepare Jwt Token
            var jwt = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                notBefore: jwtOptions.NotBefore,
                expires: jwtOptions.Expires,
                signingCredentials: jwtOptions.SigningCredentials);

            // Serialize token
            var result = new JwtSecurityTokenHandler().WriteToken(jwt);

            return result;

        }
    }
}