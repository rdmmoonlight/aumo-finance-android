package com.aumofinance.app.crashlog

import android.os.Bundle
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R
import java.io.File

// Menampilkan log crash lokal (uncaught exceptions) untuk keperluan debugging
// pengguna. Sumbernya file yang sama yang ditulis CrashLogHandler.
class CrashLogActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_crash_log)

        val logFile = File(filesDir, "crash_log.txt")
        val content = if (logFile.exists()) logFile.readText() else "Belum ada crash log."
        findViewById<TextView>(R.id.textCrashLogContent).text = content
    }
}
