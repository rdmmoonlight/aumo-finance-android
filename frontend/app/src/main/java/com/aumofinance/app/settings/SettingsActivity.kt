package com.aumofinance.app.settings

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.Switch
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.BuildConfig
import com.aumofinance.app.crashlog.CrashLogActivity
import com.aumofinance.app.network.SessionManager
import com.aumofinance.app.update.AppUpdateService
import com.aumofinance.app.R

// Halaman Settings: preferensi notifikasi (disimpan lokal lewat
// SharedPreferences — belum ada backend untuk ini, murni preferensi
// perangkat), akses Crash Log, dan tombol Logout.
class SettingsActivity : AppCompatActivity() {

    companion object {
        private const val PREFS_NAME = "aumo_settings"
        private const val KEY_NOTIFICATIONS_ENABLED = "notifications_enabled"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_settings)

        val prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)
        val switchNotifications = findViewById<Switch>(R.id.switchNotifications)
        switchNotifications.isChecked = prefs.getBoolean(KEY_NOTIFICATIONS_ENABLED, true)
        switchNotifications.setOnCheckedChangeListener { _, isChecked ->
            prefs.edit().putBoolean(KEY_NOTIFICATIONS_ENABLED, isChecked).apply()
        }

        findViewById<Button>(R.id.buttonCrashLog).setOnClickListener {
            startActivity(Intent(this, CrashLogActivity::class.java))
        }

        val updatePrefs = getSharedPreferences(AppUpdateService.PREFS_NAME, Context.MODE_PRIVATE)
        val switchAutoUpdate = findViewById<Switch>(R.id.switchAutoUpdate)
        switchAutoUpdate.isChecked = updatePrefs.getBoolean(AppUpdateService.KEY_AUTO_UPDATE_ENABLED, true)
        switchAutoUpdate.setOnCheckedChangeListener { _, isChecked ->
            updatePrefs.edit().putBoolean(AppUpdateService.KEY_AUTO_UPDATE_ENABLED, isChecked).apply()
        }

        findViewById<Button>(R.id.buttonLogout).setOnClickListener {
            startActivity(Intent(this, LogoutActivity::class.java))
        }

        findViewById<TextView>(R.id.textLoggedInAs).text =
            "Masuk sebagai ${SessionManager.fullName ?: "-"}"
        findViewById<TextView>(R.id.textAppVersion).text =
            "Versi ${BuildConfig.VERSION_NAME} (${BuildConfig.VERSION_CODE})"
    }
}
