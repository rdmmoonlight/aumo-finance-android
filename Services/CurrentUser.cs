namespace AumoFinance.Services;

/// <summary>
/// Aplikasi ini dipakai satu pengguna (single-user, tidak ada login/multi-akun),
/// jadi seluruh data (Period, Account, JournalEntry, dst.) memakai satu UserId
/// tetap ini. Didaftarkan sebagai singleton di MauiProgram agar Shell dapat
/// meng-inject-nya secara otomatis ke constructor Page yang membutuhkan
/// "Guid currentUserId".
/// </summary>
public static class CurrentUser
{
    public static readonly Guid Id = Guid.Empty;
}
