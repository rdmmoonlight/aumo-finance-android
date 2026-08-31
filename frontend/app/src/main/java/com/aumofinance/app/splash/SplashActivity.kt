package com.aumofinance.app.splash

import android.content.Intent
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.auth.LoginActivity
import com.aumofinance.app.R

// Splash screen custom (bukan API splash minimalis Android 12+) karena
// desainnya penuh — logo + teks atribusi "by rdmmoonlight" — bukan cuma
// ikon kecil di kotak seperti yang dipaksakan API splash bawaan. Ini
// sekarang jadi LAUNCHER activity (menggantikan LoginActivity langsung).
// TODO: kalau nanti sesi login dibuat persisten (lihat SessionManager),
// splash ini bisa langsung ke HomeActivity kalau token masih valid,
// tanpa lewat LoginActivity dulu.
class SplashActivity : AppCompatActivity() {

    companion object {
        private const val SPLASH_DURATION_MS = 1200L
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_splash)

        Handler(Looper.getMainLooper()).postDelayed({
            startActivity(Intent(this, LoginActivity::class.java))
            finish()
        }, SPLASH_DURATION_MS)
    }
}
