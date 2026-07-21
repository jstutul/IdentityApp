using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RCPracticeController : ControllerBase
    {
        [HttpGet("player-role")]
        [Authorize(Roles ="Player")]
        public IActionResult PlayerRole()
        {
            return Ok("Player");
        }

        [HttpGet("player-policy")]
        [Authorize(policy: "PlayerPolicy")]
        public IActionResult PlayerPolicy()
        {
            return Ok("Player Policy");
        }

        [HttpGet("admin-email-policy")]
        [Authorize(policy: "AdminEmailPolicy")]
        public IActionResult AdminEmailPolicy()
        {
            return Ok("admin email Policy");
        }
    }
}
