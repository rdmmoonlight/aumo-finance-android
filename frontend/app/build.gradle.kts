plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

android {
    // namespace = paket Kotlin (R class, dst.) — BEBAS beda dari applicationId
    // sejak AGP 7+ memisahkan keduanya, jadi tidak perlu rename seluruh
    // struktur package Kotlin yang sudah ditulis.
    namespace = "com.aumofinance.app"
    compileSdk = 34

    defaultConfig {
        // HARUS "com.bnrc.aumofinance" — ini applicationId asli app MAUI lama
        // (lihat frontend/legacy-maui-reference/AumoFinance.csproj). Kalau beda,
        // Play Store akan menganggap ini aplikasi baru yang terpisah, bukan
        // update dari app existing, dan user lama kehilangan kontinuitas rilis.
        applicationId = "com.bnrc.aumofinance"
        // Locked per project requirement: minSdk must stay at Android 9 (API 28) or below
        minSdk = 28
        targetSdk = 34
        // Bisa dioverride dari CI lewat -PappVersionCode=... -PappVersionName=...
        // (lihat android-build.yml) supaya APK punya versi internal yang
        // konsisten dengan tag GitHub Release yang dipublikasikan — bukan
        // cuma "1.0" statis selamanya. Default di bawah dipakai untuk build
        // lokal (Android Studio/gradlew tanpa CI).
        versionCode = (project.findProperty("appVersionCode") as String?)?.toIntOrNull() ?: 1
        versionName = project.findProperty("appVersionName") as String? ?: "1.0-local"
    }

    buildTypes {
        debug {
            // Signature debug (auto-generated Android debug keystore) TIDAK
            // PERNAH sama dengan keystore signing release — kalau applicationId
            // debug sama persis dengan release, install APK debug di HP yang
            // sudah ada app release akan ditolak Android ("package conflicts
            // with an existing package"). applicationIdSuffix membuat debug
            // punya package ID sendiri (com.bnrc.aumofinance.debug) supaya
            // keduanya bisa terpasang berdampingan tanpa bentrok.
            applicationIdSuffix = ".debug"
            versionNameSuffix = "-debug"
        }
        release {
            isMinifyEnabled = false
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = "17"
    }

    // AGP 8+ mewajibkan opt-in eksplisit ini untuk generate kelas BuildConfig.
    // Ditambahkan juga dukungan Compose agar bisa digunakan bersama XML secara bertahap.
    buildFeatures {
        buildConfig = true
        compose = true
    }

    // Disesuaikan dengan versi Kotlin 1.9.24
    composeOptions {
        kotlinCompilerExtensionVersion = "1.5.14"
    }
}

dependencies {
    implementation("androidx.core:core-ktx:1.13.1")
    implementation("androidx.appcompat:appcompat:1.7.0")
    implementation("com.google.android.material:material:1.12.0")
    implementation("androidx.constraintlayout:constraintlayout:2.1.4")
    implementation("androidx.recyclerview:recyclerview:1.3.2")
    implementation("androidx.lifecycle:lifecycle-viewmodel-ktx:2.8.4")
    implementation("androidx.activity:activity-ktx:1.9.1")
    implementation("com.squareup.retrofit2:retrofit:2.11.0")
    implementation("com.squareup.retrofit2:converter-gson:2.11.0")
    implementation("com.squareup.okhttp3:okhttp:4.12.0")
    implementation("com.squareup.okhttp3:logging-interceptor:4.12.0")
    // Persistensi sesi terenkripsi (EncryptedSharedPreferences) untuk
    // "Ingat saya", dan BiometricPrompt untuk login sidik jari/wajah.
    implementation("androidx.security:security-crypto:1.1.0-alpha06")
    implementation("androidx.biometric:biometric:1.1.0")

    // --- INTEGRASI JETPACK COMPOSE (MIGRASI BERTAHAP) ---
    val composeBom = platform("androidx.compose:compose-bom:2024.05.00")
    implementation(composeBom)
    androidTestImplementation(composeBom)

    implementation("androidx.compose.ui:ui")
    implementation("androidx.compose.ui:ui-graphics")
    implementation("androidx.compose.ui:ui-tooling-preview")
    implementation("androidx.compose.material3:material3")
    // Dipakai untuk ikon outline (Dashboard, MenuBook, CalendarMonth, dll)
    // di Home page — tidak tersedia di paket ikon inti material3.
    implementation("androidx.compose.material:material-icons-extended")
    implementation("androidx.activity:activity-compose:1.9.1")
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.8.4")
    debugImplementation("androidx.compose.ui:ui-tooling")

    // Tabler Icons for Compose — provides TablerIcons used in HomeScreen
    implementation("br.com.devsrsouza.compose.icons:tabler:0.2.0")
}
