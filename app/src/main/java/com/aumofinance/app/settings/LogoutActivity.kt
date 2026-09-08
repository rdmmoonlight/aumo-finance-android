package com.aumofinance.app.settings

import android.content.Intent
import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R
import com.aumofinance.app.auth.AuthApi
import com.aumofinance.app.auth.LoginActivity
import com.aumofinance.app.network.SessionManager
import com.aumofinance.app.network.SessionStore
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

class LogoutActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_logout)

        // JWT bersifat stateless di sisi server (lihat AuthController.Logout di
        // aumo-finance-web) — panggilan ini hanya formalitas, sesi sebenarnya
        // berakhir begitu token dihapus dari SessionManager di bawah. Fire-
        // and-forget: Activity langsung pindah ke Login tanpa menunggu hasil.
        CoroutineScope(Dispatchers.IO).launch {
            runCatching { AuthApi().logout() }
        }

        SessionManager.clear()
        // Hapus juga sesi terenkripsi yang tersimpan ("Ingat saya"/biometrik)
        // — tanpa ini, SplashActivity akan tetap otomatis login lagi pakai
        // sesi lama walau user sudah eksplisit logout.
        SessionStore.clear()

        val intent = Intent(this, LoginActivity::class.java)
        intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
        startActivity(intent)
        finish()
    }
}
