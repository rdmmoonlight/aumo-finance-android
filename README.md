# AumoFinance — Android (Kotlin native)

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

## Menjalankan frontend

Buka folder `frontend/` di Android Studio. Gradle Wrapper (`gradlew`) belum
digenerate di repo ini — jalankan sekali di Android Studio (otomatis) atau
manual dengan:

```
cd frontend
gradle wrapper --gradle-version 8.7
```

lalu commit `gradlew`, `gradlew.bat`, dan `gradle/wrapper/`.

`minSdk` dikunci di Android 9 (API 28) dan tidak boleh dinaikkan tanpa
instruksi eksplisit.

## Peta pengerjaan

Lihat [`PHASES.md`](./PHASES.md) untuk status tiap fase migrasi dan daftar
utang teknis yang masih terbuka.

## Batasan platform

Aplikasi ini hanya menyasar Android.
