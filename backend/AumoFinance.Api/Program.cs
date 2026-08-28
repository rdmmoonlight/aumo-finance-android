using System.Text;
using AumoFinance.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: begitu controller dipindah dari List statis in-memory ke EF Core,
// ganti isi ConnectionStrings:DefaultConnection di appsettings.Development.json
// dan (lewat env var/secret) di production, lalu jalankan migrasi awal.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Catatan: pakai IsNullOrWhiteSpace, bukan "??" — appsettings.json produksi sengaja
// mengisi Jwt:Key dengan string kosong (bukan menghapus key-nya), jadi "??" saja
// tidak akan pernah jatuh ke fallback dev.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = "dev-only-placeholder-key-change-me-in-appsettings";
}
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AumoFinance",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "AumoFinance.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
