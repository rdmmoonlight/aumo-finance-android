using System.Text;
using System.Text.Json.Serialization;
using AumoFinance.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// PENTING: System.Text.Json (default ASP.NET Core) menyerialisasi enum sebagai
// ANGKA (0,1,2,...) kalau tidak dikonfigurasi. Semua field enum (JournalType,
// AccountType, AccountCategory) di response API ini dikonsumsi Kotlin sebagai
// String ("General", "Asset", dst.) — tanpa converter ini semua DTO yang
// mengandung enum akan salah/rusak di sisi Android.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: begitu migrasi awal dijalankan, hapus komentar ini. Connection string
// produksi diisi lewat env var Render (ConnectionStrings__DefaultConnection),
// BUKAN dari appsettings.json yang di-commit.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
