using AumoFinance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Data;

// DbContext nyata, disiapkan agar controller (saat ini masih pakai List statis
// in-memory sebagai jembatan sementara) tinggal dipindah ke sini satu per satu.
// Terhubung lewat ConnectionStrings:DefaultConnection di appsettings — belum ada
// migrasi EF Core, jalankan `dotnet ef migrations add Initial` setelah skema final.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Period> Periods => Set<Period>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
}
