# AumoFinance — Peta Migrasi ke Kotlin Native

**Koreksi arah penting:** ini migrasi FRONTEND SAJA. Backend tidak dibuat baru
— aplikasi ini konsumen dari `api/mobile/*` yang sudah lengkap & stabil di
`aumo-finance-web` (https://aumo.onrender.com). Fase 1–7.2 sebelumnya sempat
membangun `/backend` baru dengan skema karangan sendiri (Account/Period/
JournalEntry versi sendiri) — **itu sudah dihapus total**. Semua lapisan API
Kotlin (folder `network/`, `auth/`, `periods/`, `coa/`, `journal/`,
`reports/`) sudah ditulis ulang untuk cocok persis dengan Models/Controllers
asli di `aumo-finance-web`.

## Perbedaan kunci dari skema karangan sebelumnya

- **Tidak ada `PeriodId` FK** — backend menentukan periode dari rentang
  tanggal `EntryDate` dibanding `Period.StartDate/EndDate`, bukan foreign key.
- **Tidak ada endpoint yang menerima parameter `periodId`** — semua laporan
  otomatis mengikuti periode yang sedang *selected* per user
  (`SelectedPeriodHelper` di backend). Yang dikirim client hanya `select/{id}`
  dan `close/{id}` untuk mengubah periode mana yang aktif.
- **Closing journal tidak pernah disimpan** — dihitung on-the-fly dari Trial
  Balance setiap kali diminta (`GET api/mobile/reports/closing-journal`).
- **Akun punya 7 kategori** (`Assets`, `Liabilities`, `Equity`,
  `OperatingIncome`, `OperatingExpenses`, `OtherIncome`, `OtherExpenses`),
  masing-masing terikat rentang nomor referensi (100-199, 200-299, dst.) —
  bukan `Category` 5-way yang saya karang sebelumnya.
- **Multi-tenant per `UserId`** — sesuatu yang skema karangan sebelumnya sama
  sekali tidak punya.
- Enum-enum backend (JournalType, Type akun) adalah **string biasa**, bukan
  C# enum — jadi tidak ada isu serialisasi enum-sebagai-angka yang perlu
  ditangani di sisi Kotlin.

## Status per lapisan Kotlin

| Lapisan API (Kotlin) | Status |
|---|---|
| Auth (login email+password, JWT Bearer via `AuthInterceptor`+`SessionManager`) | ✅ Ditulis ulang |
| Periods (list/create/select/close) | ✅ Ditulis ulang |
| Chart of Accounts (ReferenceNumber/Type/Role) | ✅ Ditulis ulang |
| Journal Entry (form create/edit/delete satu entri) | ✅ Ditulis ulang |
| General Journal / Adjusting Journal (laporan daftar entri) | ✅ Ditulis ulang |
| General Ledger (satu endpoint, query `isTemporary`) | ✅ Ditulis ulang |
| Trial Balance / Adjusted / Post-Closing (satu endpoint, query `type`) | ✅ Ditulis ulang |
| Worksheet | ✅ Ditulis ulang |
| Income Statement, Retained Earnings, Statement of Financial Position, Cash Flow, Closing Journal | ✅ Ditulis ulang |
| Dashboard | ✅ Ditulis ulang |

## Fase 8 — UI (layout + adapter + binding data)

| Langkah | Layar | Status |
|---|---|---|
| 8.1 | Dashboard (bind angka + RecyclerView Kas/Bank), Periods (list + select/close + dialog buka periode baru) | ✅ Selesai |
| 8.2 | Chart of Accounts (list + form tambah/edit) | ✅ Selesai |
| 8.3 | Journal Entry (form baris debit/kredit dinamis) | ✅ Selesai |
| 8.4 | General/Adjusting Journal report (RecyclerView berkelompok per tanggal) | ✅ Selesai |
| 8.5 | General Ledger, Trial Balance, Worksheet (tabel) | ✅ Selesai (+ tambah Post-Closing Trial Balance yang baru ditemukan di kontrak asli) |
| 8.6 | Income Statement, Retained Earnings, Financial Position, Cash Flow, Closing Journal | Belum |

## Utang teknis yang masih terbuka

- **UI sisa (8.6) belum dibangun** — lihat tabel Fase 8 di atas.
- **`gradlew` (Gradle wrapper) belum digenerate** — CI Android sementara
  memakai `gradle/actions/setup-gradle`. Setelah wrapper digenerate dan
  dicommit (lihat README), ganti kembali ke `./gradlew` di
  `frontend-android-ci.yml`.
- **Sesi login belum persisten** — `SessionManager` menyimpan token di
  memori saja, hilang begitu proses aplikasi mati. Perlu
  EncryptedSharedPreferences.
- **Sinkronisasi offline** (`SyncManager`) masih no-op.
- **`production-pipeline.yml`** masih membangun & menandatangani APK MAUI
  lama — belum disentuh, menyangkut proses rilis produksi, perlu keputusan
  eksplisit kapan diganti.
- **Keystore signing lama** sudah dihapus dari working tree; versi lama di
  git history masih perlu dibersihkan lewat `git filter-repo`/BFG kalau mau
  benar-benar hilang.
