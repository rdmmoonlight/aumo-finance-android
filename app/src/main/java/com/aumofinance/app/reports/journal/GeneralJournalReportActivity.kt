package com.aumofinance.app.reports.journal

import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import com.aumofinance.app.journal.JournalEntryActivity
import com.aumofinance.app.ui.theme.AumoTheme

// Host Compose tipis — semua tampilan ada di JournalReportScreen.kt, state
// ada di JournalReportViewModel. Sebelumnya berbasis RecyclerView + View/XML
// biasa (activity_general_journal_report.xml + JournalReportAdapter, sudah
// dihapus); dipindah ke Jetpack Compose menyusul Journal Entry, Home, dan
// Periods.
class GeneralJournalReportActivity : ComponentActivity() {
    private val viewModel: JournalReportViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        viewModel.loadGeneral()

        setContent {
            var showActions by remember { mutableStateOf(false) }
            val toastMessage = viewModel.toastMessage

            // LaunchedEffect supaya Toast hanya muncul SEKALI saat pesan berubah,
            // bukan berulang setiap recomposition.
            LaunchedEffect(toastMessage) {
                toastMessage?.let { message ->
                    Toast.makeText(this@GeneralJournalReportActivity, message, Toast.LENGTH_LONG).show()
                    viewModel.clearToast()
                }
            }

            AumoTheme {
                JournalReportScreen(
                    entries = viewModel.entries,
                    selectedPeriodName = viewModel.selectedPeriodName,
                    defaultPeriodLabel = "No period selected",
                    showToggle = true,
                    showActions = showActions,
                    onToggleShowActions = { showActions = it },
                    onEdit = { entry -> openEdit(entry.id) },
                    onDeleteRequest = { entry -> confirmDelete(entry) }
                )
            }
        }
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadGeneral()
    }

    private fun openEdit(entryId: Int) {
        startActivity(Intent(this, JournalEntryActivity::class.java).apply {
            putExtra(JournalEntryActivity.EXTRA_ENTRY_ID, entryId)
        })
    }

    // Dialog konfirmasi native (bukan Compose) sudah cukup untuk aksi
    // sekali-tap sederhana seperti ini — tidak perlu jadi bagian dari
    // JournalReportScreen composable.
    private fun confirmDelete(entry: JournalReportEntry) {
        AlertDialog.Builder(this)
            .setTitle("Delete Entry?")
            .setMessage("Entry \"${entry.transactionNumber}\" will be permanently deleted. Continue?")
            .setPositiveButton("Delete") { _, _ -> viewModel.delete(entry) }
            .setNegativeButton("Cancel", null)
            .show()
    }
}
