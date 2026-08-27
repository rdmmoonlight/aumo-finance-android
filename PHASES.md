# AumoFinance — Peta Fase Migrasi (Kotlin Native + C# Backend)

Struktur repo: `/frontend` (Android Kotlin native, tanpa MAUI) dan `/backend` (ASP.NET Core Web API, C# full). Kode MAUI lama diarsipkan di `frontend/legacy-maui-reference/` sebagai referensi logika bisnis, bukan untuk dipakai langsung.

Urutan fase dari yang termudah, beban kerja dibagi rata per fase (target ±sama banyak file/effort):

| Fase | Cakupan | Kompleksitas | Status |
|---|---|---|---|
| **1. Fondasi & Struktur** | Branch, folder `/frontend` + `/backend`, skeleton Gradle (Kotlin) & ASP.NET Core project, arsip kode MAUI lama, palet warna & tema dasar | Termudah | ✅ Selesai (commit ini) |
| **2. Auth & Layout Dasar** | LoginActivity, MainActivity/Navigation dasar (Kotlin); AuthController + JWT nyata (C#) | Mudah | ✅ Selesai |
| **3. Halaman Inti** | Home, Dashboard, Periods, Chart of Accounts, Journal Entry (Kotlin Activities/Fragments + ViewModel); controller terkait di backend | Sedang | ✅ Selesai |
| **4. Laporan Jurnal & Ledger** | General/Adjusting Journal report, General Ledger (Permanent & Temporary) | Sedang | Belum |
| **5. Trial Balance & Worksheet** | Trial Balance, Adjusted Trial Balance, Worksheet | Sedang-Tinggi | Belum |
| **6. Laporan Keuangan & Penyelesaian** | Income Statement, Statement of Financial Position, Retained Earnings, Cash Flow, Closing Journal; Settings, Crash Log, sync, CI Android + CI backend | Tertinggi | Belum |

Setiap fase = satu commit + push ke branch `feature/kotlin-native-frontend`, tidak digabung ke `main`/`production` tanpa instruksi eksplisit.

Batasan yang tetap berlaku:
- `minSdk` Android dikunci di API 28 (Android 9), tidak boleh naik.
- Platform target: Android saja.
- Ikon: Tabler Icons di seluruh frontend.
