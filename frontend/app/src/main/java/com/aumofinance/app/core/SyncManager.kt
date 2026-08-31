package com.aumofinance.app.core

// Placeholder sinkronisasi data offline-first.
// TODO (pekerjaan lanjutan di luar 6 fase ini): implementasi antrian retry untuk
// operasi yang gagal saat offline (Journal Entry, COA, Periods), dan indikator
// status "Syncing" di TopBarView seperti pada versi MAUI lama.
object SyncManager {
    fun syncPendingChanges() {
        // no-op untuk saat ini — aplikasi bersifat online-only di fase 1-6
    }
}
