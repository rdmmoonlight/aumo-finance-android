package com.aumofinance.app.settings

import android.content.Context
import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import com.aumofinance.app.crashlog.CrashLogActivity
import com.aumofinance.app.logout.LogoutActivity
import com.aumofinance.app.ui.theme.AumoTheme
import com.aumofinance.app.update.AppUpdateService

// Halaman Settings: preferensi notifikasi (disimpan lokal lewat
// SharedPreferences — belum ada backend untuk ini, murni preferensi
// perangkat), akses Crash Log, dan tombol Logout.
class SettingsActivity : ComponentActivity() {
    companion object {
        private const val PREFS_NAME = "aumo_settings"
        private const val KEY_NOTIFICATIONS_ENABLED = "notifications_enabled"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)
        val updatePrefs = getSharedPreferences(AppUpdateService.PREFS_NAME, Context.MODE_PRIVATE)

        setContent {
            var notificationsEnabled by remember {
                mutableStateOf(prefs.getBoolean(KEY_NOTIFICATIONS_ENABLED, true))
            }
            var autoUpdateEnabled by remember {
                mutableStateOf(updatePrefs.getBoolean(AppUpdateService.KEY_AUTO_UPDATE_ENABLED, true))
            }

            AumoTheme {
                SettingsScreen(
                    notificationsEnabled = notificationsEnabled,
                    onNotificationsChange = { checked ->
                        notificationsEnabled = checked
                        prefs.edit().putBoolean(KEY_NOTIFICATIONS_ENABLED, checked).apply()
                    },
                    autoUpdateEnabled = autoUpdateEnabled,
                    onAutoUpdateChange = { checked ->
                        autoUpdateEnabled = checked
                        updatePrefs.edit().putBoolean(AppUpdateService.KEY_AUTO_UPDATE_ENABLED, checked).apply()
                    },
                    onCrashLogClick = { startActivity(Intent(this, CrashLogActivity::class.java)) },
                    onLogoutClick = { startActivity(Intent(this, LogoutActivity::class.java)) },
                )
            }
        }
    }
}
