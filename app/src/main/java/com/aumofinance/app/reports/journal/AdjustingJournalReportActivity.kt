package com.aumofinance.app.reports.journal

import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.runtime.LaunchedEffect
import com.aumofinance.app.journal.JournalEntryActivity
import com.aumofinance.app.ui.theme.AumoTheme

// Sama seperti General Journal, tapi backend sudah memfilter journalType
// "Adjusting" saja; selalu menampilkan tombol edit/delete (tanpa toggle
// Edit/Selesai terpisah seperti General Journal). Host Compose tipis —
// semua tampilan ada di JournalReportScreen.kt, state ada di
// JournalReportViewModel. Sebelumnya berbasis RecyclerView + View/XML biasa
// (activity_adjusting_journal_report.xml + JournalReportAdapter, sudah
// dihapus); dipindah ke Jetpack Compose menyusul Journal Entry, Home, dan
// Periods.
class AdjustingJournalReportActivity : ComponentActivity() {
    private val viewModel: JournalReportViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        viewModel.loadAdjusting()

        setContent {
            val toastMessage = viewModel.toastMessage

            LaunchedEffect(toastMessage) {
                toastMessage?.let { message ->
                    Toast.makeText(this@AdjustingJournalReportActivity, message, Toast.LENGTH_LONG).show()
                    viewModel.clearToast()
                }
            }

            AumoTheme {
                JournalReportScreen(
                    entries = viewModel.entries,
                    selectedPeriodName = viewModel.selectedPeriodName,
                    defaultPeriodLabel = "Belum ada periode dipilih",
                    showToggle = false,
                    showActions = true,
                    onToggleShowActions = {},
                    onEdit = { entry -> openEdit(entry.id) },
                    onDeleteRequest = { entry -> confirmDelete(entry) },
                )
            }
        }
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadAdjusting()
    }

    private fun openEdit(entryId: Int) {
        startActivity(
            Intent(this, JournalEntryActivity::class.java).apply {
                putExtra(JournalEntryActivity.EXTRA_ENTRY_ID, entryId)
            },
        )
    }

    private fun confirmDelete(entry: JournalReportEntry) {
        AlertDialog.Builder(this)
            .setTitle("Hapus Entri?")
            .setMessage("Entri \"${entry.transactionNumber}\" akan dihapus permanen. Lanjutkan?")
            .setPositiveButton("Hapus") { _, _ -> viewModel.delete(entry) }
            .setNegativeButton("Batal", null)
            .show()
    }
}
