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
        versionCode = 1
        versionName = "1.0"
    }

    buildTypes {
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
    implementation("com.squareup.okhttp3:logging-interceptor:4.12.0")
}
