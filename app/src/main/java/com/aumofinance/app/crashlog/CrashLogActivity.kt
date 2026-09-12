package com.aumofinance.app.crashlog

import android.content.ClipData
import android.content.ClipboardManager
import android.content.Context
import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.aumofinance.app.ui.theme.AumoTheme
import java.io.File

class CrashLogActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val logFile = File(filesDir, "crash_log.txt")
        val content = if (logFile.exists()) logFile.readText() else "Belum ada crash log."

        setContent {
            AumoTheme {
                CrashLogScreen(
                    content = content,
                    onCopyClick = {
                        if (content.isNotEmpty() && content != "Belum ada crash log.") {
                            val clipboard = getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
                            val clip = ClipData.newPlainText("Crash Log", content)
                            clipboard.setPrimaryClip(clip)
                            Toast.makeText(this, "Crash log berhasil disalin!", Toast.LENGTH_SHORT).show()
                        } else {
                            Toast.makeText(this, "Tidak ada log untuk disalin.", Toast.LENGTH_SHORT).show()
                        }
                    },
                )
            }
        }
    }
}
