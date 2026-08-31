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
| 8.6 | Income Statement, Retained Earnings, Financial Position, Cash Flow, Closing Journal | ✅ Selesai |

**Fase 8 tuntas — seluruh 20 layar sekarang punya UI fungsional (bukan `FrameLayout` kosong lagi).**

## Utang teknis yang masih terbuka
- **Statement of Financial Position belum punya Activity untuk varian
  `isPostClosing=true`** — endpoint sudah ada dan bisa dipanggil
  (`loadFinancialPosition(isPostClosing = true)`), tapi belum ada layar
  terpisah yang memakainya, mirip Post-Closing Trial Balance di Fase 8.5.

## Fase 9 — Build via GitHub Actions (tanpa Android Studio)

- **Gradle Wrapper asli sudah ada** (`gradlew`, `gradlew.bat`,
  `gradle/wrapper/gradle-wrapper.jar` + `.properties`, Gradle 8.7) — diambil
  dari repo resmi `gradle/gradle` tag `v8.7.0` via `raw.githubusercontent.com`
  (bukan ditulis manual), karena sandbox saya tidak punya akses ke server
  distribusi Gradle resmi. `frontend-android-ci.yml` sekarang pakai
  `./gradlew` sungguhan, bukan lagi workaround `gradle/actions/setup-gradle`
  tanpa wrapper.
- **`applicationId` diperbaiki** dari `com.aumofinance.app` (karangan Fase 1)
  menjadi `com.bnrc.aumofinance` — itu applicationId asli app MAUI lama
  (lihat `frontend/legacy-maui-reference/AumoFinance.csproj`). Kalau tidak
  disamakan, rilis Kotlin ini akan dianggap aplikasi BARU oleh Play Store,
  bukan update dari app existing. `namespace` (struktur package Kotlin) tetap
  `com.aumofinance.app` — aman berbeda dari `applicationId` sejak AGP 7+.
- **`production-pipeline.yml` (MAUI) DIHAPUS**, diganti
  `.github/workflows/android-build.yml`:
  - Job `build`: jalan di setiap push ke `feature/kotlin-native-frontend`,
    hasilkan APK debug + release (unsigned) sebagai artifact — ini yang
    dipakai untuk build tanpa Android Studio.
  - Job `sign-and-release`: hanya jalan lewat trigger manual
    (`workflow_dispatch`), dan otomatis di-skip dengan pesan jelas kalau
    secret `ANDROID_KEYSTORE_BASE64` belum di-set — tidak dipaksakan jalan
    dengan kredensial yang belum tentu ada.

### Fase 9.1 — Perbaikan build pertama: `gradle.properties` hilang

Build pertama lewat `android-build.yml` gagal di task
`:app:checkDebugAarMetadata` dengan pesan:
```
Configuration `:app:debugRuntimeClasspath` contains AndroidX dependencies,
but the `android.useAndroidX` property is not enabled
```
Penyebab: `frontend/gradle.properties` tidak pernah dibuat sejak Fase 1,
padahal project ini pakai AndroidX di semua tempat (`androidx.core`,
`androidx.appcompat`, `com.google.android.material`, dst.). Ditambahkan
`android.useAndroidX=true` + `android.nonTransitiveRClass=true`.

Ini baru gagal di tahap metadata check, SEBELUM kompilasi Kotlin
sesungguhnya berjalan — jadi masih mungkin ada error lain menyusul begitu
tahap ini lolos dan compiler Kotlin benar-benar jalan untuk pertama kalinya.

**Update:** setelah fix ini di-push, build ke-2 sukses penuh (kompilasi
Kotlin lolos tanpa error lain) — lihat run
https://github.com/rdmmoonlight/aumo-finance-android/actions/runs/33268437916.
Kedua workflow (`android-build.yml` dan `frontend-android-ci.yml`) hijau.
Project ini sekarang **bisa dibangun murni lewat GitHub Actions, tanpa
Android Studio**, sesuai permintaan.

## Utang teknis lain yang masih terbuka

- **Sesi login belum persisten** — `SessionManager` menyimpan token di
  memori saja, hilang begitu proses aplikasi mati. Perlu
  EncryptedSharedPreferences.
- **Sinkronisasi offline** (`SyncManager`) masih no-op.
- **Signing release APK belum aktif** — job `sign-and-release` sudah siap,
  tapi perlu secret `ANDROID_KEYSTORE_BASE64`/`ANDROID_KEY_ALIAS`/
  `ANDROID_KEY_PASSWORD`/`ANDROID_KEYSTORE_PASSWORD` di-set dulu di
  pengaturan repo (Settings → Secrets and variables → Actions). Kalau
  keystore lama (`.jks` yang sempat ada di `Docs/`) masih dipakai, pastikan
  itu didaftarkan sebagai secret, bukan file — jangan commit `.jks` lagi.
- **Keystore signing lama** sudah dihapus dari working tree; versi lama di
  git history masih perlu dibersihkan lewat `git filter-repo`/BFG kalau mau
  benar-benar hilang.
- **`android-build.yml` belum menyasar branch `production`** — sengaja
  di-scope ke `feature/kotlin-native-frontend` dulu sesuai instruksi "sampai
  bisa build di branch ini saja"; perluas trigger-nya setelah migrasi ini
  siap dirilis.
