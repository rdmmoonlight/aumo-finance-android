package com.aumofinance.app.logout

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.aumofinance.app.auth.AuthApi
import com.aumofinance.app.auth.LoginActivity
import com.aumofinance.app.network.SessionManager
import com.aumofinance.app.network.SessionStore
import com.aumofinance.app.ui.theme.AumoColors
import com.aumofinance.app.ui.theme.AumoTheme
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

// Tidak ada konten yang benar-benar ditampilkan (activity_logout.xml lama
// juga kosong, hanya latar belakang) — tetap pakai setContent supaya tidak
// nge-flash putih default sebelum pindah ke Login, dan agar tidak ada
// dependensi XML sama sekali di halaman ini.
class LogoutActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent { LogoutScreen() }

        // JWT bersifat stateless di sisi server — panggilan ini hanya
        // formalitas (mencatat aktivitas logout di fitur Guardian), sesi
        // sebenarnya berakhir begitu token dihapus dari SessionManager di
        // bawah. Fire-and-forget: Activity langsung pindah ke Login tanpa
        // menunggu hasil.
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

@Composable
private fun LogoutScreen() {
    AumoTheme {
        Box(modifier = Modifier.fillMaxSize().background(AumoColors.Background))
    }
}
