package com.aumofinance.app.settings

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Halaman Settings: preferensi notifikasi, info akun, tombol Logout, akses Crash Log.
class SettingsActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_settings)
        // TODO: wire up switch notifikasi, tombol Logout -> LogoutActivity, tombol Crash Log -> CrashLogActivity
    }
}
