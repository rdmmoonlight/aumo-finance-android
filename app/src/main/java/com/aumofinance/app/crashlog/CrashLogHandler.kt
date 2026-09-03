package com.aumofinance.app.crashlog

import android.content.Context
import java.io.File
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

// Uncaught exception handler sederhana: menulis stack trace ke file lokal
// agar bisa dilihat lagi lewat CrashLogActivity, mengganti default Android crash dialog.
class CrashLogHandler(private val context: Context) : Thread.UncaughtExceptionHandler {
    private val defaultHandler = Thread.getDefaultUncaughtExceptionHandler()

    override fun uncaughtException(thread: Thread, throwable: Throwable) {
        try {
            val logFile = File(context.filesDir, "crash_log.txt")
            val timestamp = SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault()).format(Date())
            logFile.appendText("\n[$timestamp]\n${throwable.stackTraceToString()}\n")
        } catch (_: Exception) {
            // Jangan biarkan logging itu sendiri menyebabkan crash tambahan.
        }
        defaultHandler?.uncaughtException(thread, throwable)
    }
}
