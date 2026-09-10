package com.aumofinance.app.settings

import android.content.Intent
import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R
import com.aumofinance.app.auth.AuthApi
import com.aumofinance.app.auth.LoginActivity
import com.aumofinance.app.network.ApiClient
import com.aumofinance.app.network.SessionManager
import com.aumofinance.app.network.SessionStore
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

class LogoutActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_logout)

        // JWT bersifat stateless di sisi server — anggapan ini SALAH sejak
        // diketahui backend pakai cookie ASP.NET Identity ("AumoFinance.
        // Session", lihat AuthController.Logout -> SignOutAsync). Panggilan
        // ke /auth/logout di sini yang justru penting (menghapus sign-in
        // stamp di server), disusul membersihkan cookie jar lokal supaya
        // tidak terkirim lagi. Fire-and-forget: Activity langsung pindah ke
        // Login tanpa menunggu hasilnya.
        CoroutineScope(Dispatchers.IO).launch {
            runCatching { AuthApi().logout() }
            ApiClient.cookiesStorage.clearAll()
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
