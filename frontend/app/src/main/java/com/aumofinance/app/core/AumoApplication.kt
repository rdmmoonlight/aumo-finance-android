package com.aumofinance.app.core

import android.app.Application
import com.aumofinance.app.crashlog.CrashLogHandler

// CrashLogHandler sebelumnya ada di codebase tapi TIDAK PERNAH benar-benar
// didaftarkan sebagai default uncaught exception handler — dead code sejak
// dibuat. Didaftarkan di sini supaya CrashLogActivity benar-benar punya isi.
class AumoApplication : Application() {
    override fun onCreate() {
        super.onCreate()
        Thread.setDefaultUncaughtExceptionHandler(CrashLogHandler(this))
    }
}
