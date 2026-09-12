package com.aumofinance.app.settings

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Switch
import androidx.compose.material3.SwitchDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.aumofinance.app.BuildConfig
import com.aumofinance.app.ui.theme.AumoColors

/**
 * Padanan Compose dari activity_settings.xml. State dua switch (Notifikasi,
 * Perbarui Otomatis) di-hoist ke SettingsActivity karena sumbernya
 * SharedPreferences (murni preferensi perangkat, belum ada backend untuk ini).
 */
@Composable
fun SettingsScreen(
    notificationsEnabled: Boolean,
    onNotificationsChange: (Boolean) -> Unit,
    autoUpdateEnabled: Boolean,
    onAutoUpdateChange: (Boolean) -> Unit,
    onCrashLogClick: () -> Unit,
    onLogoutClick: () -> Unit,
) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(
            modifier =
                Modifier
                    .fillMaxSize()
                    .padding(innerPadding)
                    .padding(16.dp),
        ) {
            Column(modifier = Modifier.weight(1f)) {
                SettingsSwitchRow(
                    label = "Notifikasi",
                    checked = notificationsEnabled,
                    onCheckedChange = onNotificationsChange,
                )
                SettingsSwitchRow(
                    label = "Perbarui Otomatis",
                    checked = autoUpdateEnabled,
                    onCheckedChange = onAutoUpdateChange,
                )

                Button(
                    onClick = onCrashLogClick,
                    colors = ButtonDefaults.buttonColors(containerColor = AumoColors.Surface),
                    modifier = Modifier.fillMaxWidth().padding(top = 12.dp),
                ) {
                    Text("Lihat Crash Log", color = AumoColors.TextPrimary)
                }

                Button(
                    onClick = onLogoutClick,
                    colors = ButtonDefaults.buttonColors(containerColor = AumoColors.Bad),
                    modifier = Modifier.fillMaxWidth().padding(top = 24.dp),
                ) {
                    Text("Logout", color = AumoColors.TextPrimary)
                }
            }

            SettingsFooter()
        }
    }
}

@Composable
private fun SettingsSwitchRow(
    label: String,
    checked: Boolean,
    onCheckedChange: (Boolean) -> Unit,
) {
    Row(
        modifier = Modifier.fillMaxWidth().padding(vertical = 14.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Text(
            text = label,
            color = AumoColors.TextPrimary,
            fontSize = MaterialTheme.typography.bodyMedium.fontSize,
            modifier = Modifier.weight(1f),
        )
        Switch(
            checked = checked,
            onCheckedChange = onCheckedChange,
            colors = SwitchDefaults.colors(checkedTrackColor = AumoColors.Primary),
        )
    }
}

@Composable
private fun SettingsFooter() {
    Column(
        modifier = Modifier.fillMaxWidth().padding(top = 16.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(10.dp),
    ) {
        androidx.compose.foundation.layout.Box(
            modifier =
                Modifier
                    .fillMaxWidth()
                    .height(1.dp)
                    .background(AumoColors.Border),
        )
        Text(
            text = "Versi ${BuildConfig.VERSION_NAME} (${BuildConfig.VERSION_CODE})",
            color = AumoColors.TextMuted,
            fontSize = MaterialTheme.typography.labelSmall.fontSize,
        )
        Text(
            text = "\u00A9 2026 rdmmoonlight",
            color = AumoColors.TextMuted,
            fontSize = MaterialTheme.typography.labelSmall.fontSize,
            modifier = Modifier.padding(bottom = 8.dp),
        )
    }
}
