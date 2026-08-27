using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers.Api;

// Phase 2: wire up real auth logic (JWT issue/validate),
// migrating behavior from the previous ASP.NET Core backend.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login() => Ok(new { message = "stub - phase 2" });
}
