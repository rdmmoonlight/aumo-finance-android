package com.aumofinance.app.crashlog

import android.content.ClipData
import android.content.ClipboardManager
import android.content.Context
import android.os.Bundle
import android.widget.Button
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R
import java.io.File

class CrashLogActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_crash_log)

        val textLog = findViewById<TextView>(R.id.textCrashLogContent)
        val btnCopy = findViewById<Button>(R.id.btnCopyCrashLog) // Pastikan ID ini ada di XML layout

        val logFile = File(filesDir, "crash_log.txt")
        val content = if (logFile.exists()) logFile.readText() else "Belum ada crash log."
        textLog.text = content

        btnCopy.setOnClickListener {
            if (content.isNotEmpty() && content != "Belum ada crash log.") {
                val clipboard = getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
                val clip = ClipData.newPlainText("Crash Log", content)
                clipboard.setPrimaryClip(clip)

                Toast.makeText(this, "Crash log berhasil disalin!", Toast.LENGTH_SHORT).show()
            } else {
                Toast.makeText(this, "Tidak ada log untuk disalin.", Toast.LENGTH_SHORT).show()
            }
        }
    }
}
