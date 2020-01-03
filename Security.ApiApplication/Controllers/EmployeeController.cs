using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Security.ApiApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EmployeeController : ControllerBase
    {
        // GET api/values
        [HttpGet("userstatus")]
        public IActionResult GetUserStatus()
        {
            var isAuthenticated = this.HttpContext.User.Identities.Any(u => u.IsAuthenticated);
            var email = this.User.FindFirst(c => c.Type.Contains("email"))?.Value;

            var result = new
            {
                IsAuthenticated = isAuthenticated,
                Email = email
            };

            return Ok(result);
        }
    }
}
