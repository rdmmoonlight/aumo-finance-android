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

## Fase 14 — Sederhanakan workflow jadi 1 APK + footer Settings

- **`android-build.yml`** disederhanakan dari 2 job (build debug+unsigned,
  lalu sign terpisah) jadi 1 job linear: build release → sign → upload
  **satu** artifact (`aumo-release-signed-vX.Y.Z`) → publish GitHub Release.
  APK debug & APK release-unsigned tidak lagi di-build/di-upload di sini —
  build+test untuk validasi CI tetap ada di `frontend-android-ci.yml`
  terpisah (assembleDebug + testDebugUnitTest), jadi cakupan tes tidak
  hilang, cuma tidak lagi duplikasi artifact yang membingungkan.
- Guard "cek secret dikonfigurasi" dihapus (sebelumnya skip diam-diam kalau
  secret kosong) — sekarang gagal jelas kalau ada yang salah, karena 4
  secret keystore sudah dikonfirmasi stabil sejak Fase 9.6.
- **Footer halaman Settings**: info user yang sedang login
  (`SessionManager.fullName`), versi app (`BuildConfig.VERSION_NAME` +
  `VERSION_CODE`), dan teks copyright statis "© 2026 rdmmoonlight
  Professional".

## Fase 13 — Auto-update (ditemukan tidak pernah ada sejak migrasi)

**Temuan:** app Kotlin ini TIDAK PERNAH punya fitur cek-update sama sekali
sejak Fase 1 — `UpdateService.cs` (app MAUI lama, cek `releases/latest`
GitHub setiap start, auto-download+install kalau ada versi lebih baru)
tidak pernah di-porting. Itu sebabnya auto-update berhenti bekerja begitu
user pindah dari app MAUI ke app Kotlin ini.

Porting persis ke `AppUpdateService.kt`, dipanggil dari `SplashActivity`
setiap app start (silent, background thread):
- `GET api.github.com/repos/rdmmoonlight/aumo-finance-android/releases/latest`
- Bandingkan `tag_name` (tanpa prefix "v") vs `BuildConfig.VERSION_NAME`
  pakai perbandingan dotted-numeric sederhana (setara `System.Version.CompareTo()`)
- Unduh asset `.apk` pertama lewat Android `DownloadManager`, install lewat
  `FileProvider` + `Intent.ACTION_VIEW` setelah unduhan selesai
- Sakelar "Perbarui Otomatis" di Settings (default aktif), disimpan
  `SharedPreferences` — nama key disamakan gaya dengan
  `Preferences.Default.Get("AutoUpdateEnabled", true)` di app lama
- **Dilewati kalau `BuildConfig.DEBUG`** — build debug punya
  `versionNameSuffix "-debug"` (mis. `"26.9.1-debug"`) yang tidak bisa
  dibandingkan apel-ke-apel dengan tag rilis GitHub

Perubahan pendukung:
- `buildFeatures { buildConfig = true }` diaktifkan (wajib eksplisit di
  AGP 8+, sebelumnya belum ada — `BuildConfig.VERSION_NAME` tidak bisa
  diakses tanpa ini)
- Manifest: permission `REQUEST_INSTALL_PACKAGES` + deklarasi
  `<provider>` FileProvider (authorities `${applicationId}.fileprovider`)
  + `res/xml/file_paths.xml` (path `Download/`, cocok dengan
  `setDestinationInExternalFilesDir(..., DIRECTORY_DOWNLOADS, ...)`)
- Dependency baru: `com.squareup.okhttp3:okhttp:4.12.0` eksplisit (sebelumnya
  cuma transitif lewat Retrofit/logging-interceptor)

## Fase 12 — "Ingat saya", login biometrik, desain ulang halaman Login

- **`SessionStore`** (baru): sesi (token/userId/fullName) disimpan terenkripsi
  (AES256-GCM via Android Keystore, lewat `EncryptedSharedPreferences`) —
  sebelumnya `SessionManager` cuma di memori, hilang tiap app di-restart
  (dicatat sebagai utang teknis sejak Fase 7). Dipulihkan otomatis di
  `SplashActivity` kalau "Ingat saya" dicentang saat login.
- **Login biometrik**: `BiometricHelper` (wrapper `BiometricPrompt`,
  `BIOMETRIC_WEAK`). Centang "Aktifkan biometrik" otomatis ikut mencentang
  "Ingat saya" (biometrik cuma jadi gerbang untuk MEMBUKA sesi yang sudah
  tersimpan, bukan pengganti password sepenuhnya).
  **Catatan jujur soal batas keamanannya**: ini BUKAN cryptographic binding
  penuh (token tidak dienkripsi pakai key yang terikat ke sensor biometrik
  lewat `CryptoObject`) — cukup untuk mencegah orang lain yang pegang HP
  tak terkunci langsung masuk tanpa sidik jari/wajah pemilik, tapi bukan
  proteksi kriptografis penuh terhadap ekstraksi token di perangkat yang
  di-root. Peningkatan ke `CryptoObject`-based binding masih utang teknis.
- **`SplashActivity`** sekarang mengecek sesi tersimpan: ada sesi + biometrik
  aktif → minta biometrik dulu (batal/gagal tetap ke Login, bukan dipaksa
  keluar app, sesi tersimpan tidak dihapus); ada sesi tanpa biometrik →
  langsung ke Home; tidak ada sesi → ke Login seperti biasa.
- **`LogoutActivity`** diperbaiki: sebelumnya cuma `SessionManager.clear()`
  (in-memory) — kalau user logout tapi sesi terenkripsi tidak ikut dihapus,
  `SplashActivity` akan otomatis login lagi pakai sesi lama di buka
  berikutnya. Ditambah `SessionStore.clear()`.
- **Desain ulang halaman Login**: logo app (`drawable/app_logo.png`, dari
  `appicon.png`, terpisah dari launcher icon supaya tidak kena masking
  adaptive icon) + nama "AumoFinance" + tagline, kartu form dengan sudut
  membulat (`bg_login_card.xml`, `bg_login_input.xml`), pesan error inline
  (bukan cuma `TODO` seperti sebelumnya — ini juga baru pertama kali
  benar-benar ditampilkan ke user, sebelumnya cuma silent `TODO`).

## Fase 11 — App icon & splash screen

Sebelumnya app TIDAK PERNAH punya `android:icon` di manifest sejak Fase 1
(pakai icon default generik Android) dan tidak punya splash screen sama
sekali. Ditambahkan dari 2 aset yang diberikan (`appicon.png` 1024x1024,
`splash.png` 941x1672):

- **App icon**: adaptive icon (`mipmap-anydpi-v26/ic_launcher.xml` +
  `ic_launcher_round.xml`) dengan background putih solid
  (`@color/ic_launcher_background`) + foreground logo "A" emas. Konten asli
  diperkecil ke 76% dan ditaruh center di kanvas 1024x1024 baru supaya
  padding merata di semua sisi (source asli agak tidak simetris — padding
  bawah cuma ~8.7%, berisiko konten terpotong mask lingkaran/squircle
  launcher tertentu). Juga digenerate versi legacy flat (komposit di atas
  putih) per density untuk fallback pre-adaptive-icon & kompatibilitas
  launcher yang tidak dukung adaptive icon.
- **Splash screen**: `SplashActivity` custom (BUKAN API splash minimalis
  Android 12+) karena desainnya penuh — logo + teks atribusi
  "by rdmmoonlight" — bukan cuma ikon kecil di kotak seperti yang dipaksakan
  API splash bawaan. Jadi LAUNCHER activity baru (menggantikan
  `LoginActivity` langsung), tampil 1.2 detik lalu pindah ke Login. Gambar
  splash dikompres dari 610KB ke 393KB (PNG palette-based) tanpa kehilangan
  kualitas visual yang terlihat, ditaruh di `drawable-nodpi` supaya tidak
  ikut sistem scaling otomatis per-density Android (gambar sudah didesain
  utuh, bukan aset density-aware).

## Fase 10 — Merge ke `main` + perbaikan navigasi kritis

Branch `feature/kotlin-native-frontend` di-merge ke `main` (commit `a291a77`)
atas instruksi eksplisit pemilik repo, SEBELUM navigasi Home selesai —
disengaja, supaya `main` langsung mencerminkan arah Kotlin native, dengan
sisa pekerjaan dilanjutkan langsung di `main` setelahnya.

**Bug kritis yang ditemukan tepat sebelum merge:** `LoginActivity` mengarah
ke `MainActivity` (shell placeholder kosong dari Fase 1), BUKAN ke
`HomeActivity` (landing page yang sudah didesain sejak Fase 3 untuk jadi
hub navigasi). Akibatnya app benar-benar buntu setelah login — layar kosong
tanpa tombol apapun, walau 20 layar lain di baliknya semua sudah berfungsi.

Diperbaiki di `main`:
- `HomeActivity` sekarang berisi 2 RecyclerView menu (Menu Utama: Dashboard/
  Periode/COA/Tambah Journal Entry; Laporan: seluruh 13 layar laporan) +
  tombol Settings di pojok kanan atas.
- `LoginActivity` diarahkan ke `HomeActivity`, bukan lagi `MainActivity`.
- `MainActivity` (placeholder kosong, sudah tidak dipakai) dihapus total
  beserta layout dan entry manifest-nya.
- `SettingsActivity` disambungkan: toggle notifikasi (disimpan lokal lewat
  SharedPreferences), tombol ke `CrashLogActivity`, tombol Logout ke
  `LogoutActivity`.
- `CrashLogActivity` dibuat benar-benar membaca `crash_log.txt`.
- Ditemukan `CrashLogHandler` tidak pernah didaftarkan sebagai default
  uncaught exception handler sejak dibuat (dead code) — dibuat
  `AumoApplication` (custom `Application` class) yang mendaftarkannya di
  `onCreate()`, didaftarkan di manifest lewat `android:name`.

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
