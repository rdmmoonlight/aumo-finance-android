using AumoFinance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Period> Periods => Set<Period>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasIndex(a => a.Code)
            .IsUnique();

        modelBuilder.Entity<JournalEntry>()
            .HasOne(e => e.Period)
            .WithMany()
            .HasForeignKey(e => e.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<JournalLine>()
            .HasOne(l => l.JournalEntry)
            .WithMany(e => e.Lines)
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JournalLine>()
            .HasOne(l => l.Account)
            .WithMany(a => a.Lines)
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kolom decimal butuh presisi eksplisit di Postgres (default 'numeric' tanpa
        // presisi bisa membulatkan/menolak nilai besar).
        modelBuilder.Entity<JournalLine>().Property(l => l.Debit).HasPrecision(18, 2);
        modelBuilder.Entity<JournalLine>().Property(l => l.Credit).HasPrecision(18, 2);
    }
}
