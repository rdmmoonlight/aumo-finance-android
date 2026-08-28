# AumoFinance

Aplikasi akuntansi Android. Branch `feature/kotlin-native-frontend` sedang
memigrasikan aplikasi dari .NET MAUI ke arsitektur baru:

```
/frontend   -> Android native, Kotlin (menggantikan MAUI)
/backend    -> ASP.NET Core Web API, C#
```

Kode MAUI lama diarsipkan di `frontend/legacy-maui-reference/` sebagai referensi
logika bisnis lama, bukan untuk dijalankan langsung.

## Menjalankan backend

```
cd backend/AumoFinance.Api
dotnet restore
dotnet run
```

Swagger UI tersedia di `/swagger` saat berjalan di mode Development.
Isi `Jwt:Key` dan `ConnectionStrings:DefaultConnection` di
`appsettings.Development.json` sebelum menjalankan secara lokal.

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
utang teknis yang masih terbuka (penyimpanan backend, gradlew, dst).

## Batasan platform

Aplikasi ini hanya menyasar Android.
