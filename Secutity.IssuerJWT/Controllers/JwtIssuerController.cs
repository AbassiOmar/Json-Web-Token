using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Secutity.IssuerJWT.Configurations;
using Secutity.IssuerJWT.Extensions;
using Secutity.IssuerJWT.Models;

namespace Secutity.IssuerJWT.Controllers
{
    public class JwtIssuerController : Controller
    {
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly IJwtIssuerOptions JwtOptions;
        private readonly RoleManager<IdentityRole> RoleManager;

        public async Task<IActionResult> Login([FromBody] LoginViewModel login)
        {
            if (login == null)
                return BadRequest("Login must be assigned");
            var user = await UserManager.FindByEmailAsync(login.Email);
            if (user == null || (!(await SignInManager.PasswordSignInAsync(user, login.Password, false, false)).Succeeded))
                return BadRequest("Invalid credentials");
            var token = await CreateJWTTokenAsync(user);
            await SignInManager.SignOutAsync();
            var result = new ContentResult() { Content = token, ContentType = "application/text" };
            return result;
        }

        private async Task<string> CreateJWTTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>(new[]
            {
                // Issuer
                new Claim(JwtRegisteredClaimNames.Iss, JwtOptions.Issuer),   

                // UserName
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),       

                // Email is unique
                new Claim(JwtRegisteredClaimNames.Email, user.Email),        

                // Unique Id for all Jwt tokes
                new Claim(JwtRegisteredClaimNames.Jti, await JwtOptions.JtiGenerator()),

                // Issued at
                new Claim(JwtRegisteredClaimNames.Iat, JwtOptions.IssuedAt.ToUnixEpochDate().ToString(), ClaimValueTypes.Integer64)
            });

            claims.AddRange(await UserManager.GetClaimsAsync(user));
            var roleNames = await UserManager.GetRolesAsync(user);
            foreach (var roleName in roleNames)
            {
                // Find IdentityRole by name
                var role = await RoleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    // Convert Identity to claim and add 
                    var roleClaim = new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, JwtOptions.Issuer);
                    claims.Add(roleClaim);

                    // Add claims belonging to the role
                    var roleClaims = await RoleManager.GetClaimsAsync(role);
                    claims.AddRange(roleClaims);
                }
            }

            // Prepare Jwt Token
            var jwt = new JwtSecurityToken(
                issuer: JwtOptions.Issuer,
                audience: JwtOptions.Audience,
                claims: claims,
                notBefore: JwtOptions.NotBefore,
                expires: JwtOptions.Expires,
                signingCredentials: JwtOptions.SigningCredentials);

            // Serialize token
            var result = new JwtSecurityTokenHandler().WriteToken(jwt);

            return result;

        }
    }
}