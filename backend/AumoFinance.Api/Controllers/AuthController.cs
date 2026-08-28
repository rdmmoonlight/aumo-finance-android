using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AumoFinance.Api.Controllers;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, DateTime ExpiresAt);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // TODO fase 3: validasi terhadap tabel Users nyata (hashing password, dsb).
        // Untuk fase 2 ini hanya menyiapkan jalur JWT end-to-end.
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new { message = "Username/password tidak valid" });
        }

        var expiresAt = DateTime.UtcNow.AddHours(12);
        var token = GenerateToken(request.Username, expiresAt);

        return Ok(new LoginResponse(token, expiresAt));
    }

    private string GenerateToken(string username, DateTime expiresAt)
    {
        // IsNullOrWhiteSpace, bukan "??" — lihat catatan di Program.cs soal
        // Jwt:Key kosong (bukan hilang) di appsettings.json produksi.
        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyString))
        {
            keyString = "dev-only-placeholder-key-change-me-in-appsettings";
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "AumoFinance",
            audience: _config["Jwt:Audience"] ?? "AumoFinance.Client",
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
