AumoFinance — Android (Kotlin native)

Ini adalah **migrasi frontend saja**: dari .NET MAUI ke Android native (Kotlin).
Backend TIDAK dibuat baru — aplikasi ini murni konsumen dari REST API mobile
yang sudah lengkap dan stabil di **`aumo-finance-web`**
(https://github.com/rdmmoonlight/aumo-finance-web), di-deploy di
`https://aumo.onrender.com`.

```
/frontend   -> Android native, Kotlin (menggantikan MAUI)
```

Kode MAUI lama diarsipkan di `frontend/legacy-maui-reference/` sebagai referensi
logika bisnis lama, bukan untuk dijalankan langsung.

## Backend

Semua endpoint yang dipakai app ini ada di bawah `api/mobile/*` pada
`aumo-finance-web` — lihat repo tersebut untuk skema database (PostgreSQL/Neon),
Models, dan Controllers yang sesungguhnya. Jangan buat backend/skema baru di
repo ini; kalau ada penyesuaian API yang diperlukan, perubahannya masuk ke
`aumo-finance-web`, bukan di sini.

Autentikasi: `POST api/mobile/auth/login` (email+password) mengembalikan JWT,
dikirim di setiap request berikutnya sebagai header `Authorization: Bearer <token>`.

## Build

Ada Gradle Wrapper asli (`gradlew`/`gradlew.bat`/`gradle/wrapper/`), jadi bisa
dibangun tanpa Android Studio:

```
cd frontend
./gradlew assembleDebug
```

`applicationId` = `com.bnrc.aumofinance` (harus persis sama dengan app MAUI
lama — lihat komentar di `app/build.gradle.kts` — supaya rilis berikutnya
tetap dianggap update, bukan aplikasi baru, oleh Play Store).

Build otomatis lewat GitHub Actions juga tersedia — lihat
`.github/workflows/android-build.yml`. Setiap push ke branch
`feature/kotlin-native-frontend` menghasilkan APK debug + release (unsigned)
sebagai artifact. Signing + publish ke GitHub Releases hanya berjalan lewat
trigger manual (`workflow_dispatch`) dan hanya jika secret
`ANDROID_KEYSTORE_BASE64` (+ `ANDROID_KEY_ALIAS`, `ANDROID_KEY_PASSWORD`,
`ANDROID_KEYSTORE_PASSWORD`) sudah di-set di repo.

`minSdk` dikunci di Android 9 (API 28) dan tidak boleh dinaikkan tanpa
instruksi eksplisit.

## Peta pengerjaan

Lihat [`PHASES.md`](./PHASES.md) untuk status tiap fase migrasi dan daftar
utang teknis yang masih terbuka.

## Batasan platform

Aplikasi ini hanya menyasar Android.
