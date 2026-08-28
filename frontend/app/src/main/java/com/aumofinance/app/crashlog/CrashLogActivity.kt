package com.aumofinance.app.crashlog

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Menampilkan log crash lokal (uncaught exceptions) untuk keperluan debugging pengguna.
class CrashLogActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_crash_log)
        // TODO: baca file log dari CrashLogHandler, tampilkan di RecyclerView/scroll view
    }
}
