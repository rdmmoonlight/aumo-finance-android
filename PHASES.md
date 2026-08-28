# AumoFinance — Peta Fase Migrasi (Kotlin Native + C# Backend)

Struktur repo: `/frontend` (Android Kotlin native, tanpa MAUI) dan `/backend` (ASP.NET Core Web API, C# full). Kode MAUI lama diarsipkan di `frontend/legacy-maui-reference/` sebagai referensi logika bisnis, bukan untuk dipakai langsung.

Urutan fase dari yang termudah, beban kerja dibagi rata per fase (target ±sama banyak file/effort):

| Fase | Cakupan | Kompleksitas | Status |
|---|---|---|---|
| **1. Fondasi & Struktur** | Branch, folder `/frontend` + `/backend`, skeleton Gradle (Kotlin) & ASP.NET Core project, arsip kode MAUI lama, palet warna & tema dasar | Termudah | ✅ Selesai (commit ini) |
| **2. Auth & Layout Dasar** | LoginActivity, MainActivity/Navigation dasar (Kotlin); AuthController + JWT nyata (C#) | Mudah | ✅ Selesai |
| **3. Halaman Inti** | Home, Dashboard, Periods, Chart of Accounts, Journal Entry (Kotlin Activities/Fragments + ViewModel); controller terkait di backend | Sedang | ✅ Selesai |
| **4. Laporan Jurnal & Ledger** | General/Adjusting Journal report, General Ledger (Permanent & Temporary) | Sedang | ✅ Selesai |
| **5. Trial Balance & Worksheet** | Trial Balance, Adjusted Trial Balance, Worksheet | Sedang-Tinggi | ✅ Selesai |
| **6. Laporan Keuangan & Penyelesaian** | Income Statement, Statement of Financial Position, Retained Earnings, Cash Flow, Closing Journal; Settings, Crash Log, sync, CI Android + CI backend | Tertinggi | ✅ Selesai |

## Utang teknis yang belum tuntas (di luar 6 fase, perlu tindak lanjut)
- **Penyimpanan backend masih in-memory** (List statis di semua controller) — `Data/AppDbContext.cs` sudah disiapkan (DbSet Accounts/Periods/JournalEntries, terdaftar di Program.cs lewat `UseSqlServer`), tapi controller belum dipindah untuk memakainya; isi `ConnectionStrings:DefaultConnection` dulu sebelum dipakai.
- **Semua laporan (Trial Balance, Worksheet, Income Statement, dst.) masih placeholder kosong** — logika perhitungan nyata menyusul setelah controller dipindah ke `AppDbContext`.
- **`gradlew` (Gradle wrapper) belum digenerate** di `/frontend` — CI Android untuk sementara memakai `gradle/actions/setup-gradle` (menginstal Gradle langsung di runner) sebagai solusi sementara; setelah wrapper digenerate dan dicommit (lihat README), ganti kembali ke `./gradlew` di `frontend-android-ci.yml`.
- **Validasi login masih stub** (belum cek ke tabel Users sungguhan dengan password hashing).
- **Sinkronisasi offline** (`SyncManager`) masih no-op — aplikasi bersifat online-only untuk saat ini.
- **Keystore signing lama** (`Docs/aumo's-release-key.jks`, `keystore-base64.txt`) masih ada di git history di bawah `frontend/legacy-maui-reference/Docs/` — sudah ditambahkan ke `.gitignore` untuk mencegah file baru, tapi versi lama di history perlu dibersihkan lewat `git filter-repo`/BFG dan dipindah ke GitHub Secrets.
- **`production-pipeline.yml`** (di `.github/workflows/`) masih membangun & menandatangani APK MAUI lama (`net10.0-android36.0`) dan hanya berjalan saat push ke branch `production`. Belum disentuh di sini karena menyangkut proses rilis/signing produksi — perlu keputusan eksplisit kapan pipeline ini diganti agar sesuai proyek Kotlin native yang baru.

Setiap fase = satu commit + push ke branch `feature/kotlin-native-frontend`, tidak digabung ke `main`/`production` tanpa instruksi eksplisit.

Batasan yang tetap berlaku:
- `minSdk` Android dikunci di API 28 (Android 9), tidak boleh naik.
- Platform target: Android saja.
- Ikon: Tabler Icons di seluruh frontend.
