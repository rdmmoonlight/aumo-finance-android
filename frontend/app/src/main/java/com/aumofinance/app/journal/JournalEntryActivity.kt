package com.aumofinance.app.journal

import android.app.DatePickerDialog
import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.ui.theme.AumoTheme
import java.util.Calendar

// Form input satu Journal Entry (create atau edit, tergantung apakah
// EXTRA_ENTRY_ID diberikan). EntryDate (tanggal manual) dan CreatedAt/UpdatedAt
// (waktu lokal perangkat) diformat TANPA info zona waktu (mis.
// "2026-08-28T14:30:00", bukan dengan sufiks "Z" atau offset) — backend
// hanya me-relabel nilai itu sebagai UTC apa adanya (DateTime.SpecifyKind),
// bukan mengonversi, jadi nilai jam dinding perangkat harus sampai persis
// sama tanpa digeser (riwayat bug lama: tanggal mundur 1 hari).
// Tidak ada field periodId — backend hanya menolak entri yang EntryDate-nya
// jatuh di periode yang sudah Closed (lihat PeriodLock.IsDateLocked).
//
// Ditulis ulang dengan Jetpack Compose (sebelumnya RecyclerView + XML) —
// Activity ini sekarang cuma host tipis: seluruh state form (journal type,
// tanggal, nomor transaksi, baris) hidup di JournalEntryViewModel, seluruh
// tampilan ada di JournalEntryScreen.
class JournalEntryActivity : ComponentActivity() {

    companion object {
        const val EXTRA_ENTRY_ID = "extra_entry_id"
    }

    private val viewModel: JournalEntryViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val entryId = intent.getIntExtra(EXTRA_ENTRY_ID, -1).takeIf { it != -1 }
        viewModel.initFor(entryId)

        setContent {
            val errorMessage by viewModel.errorMessage.observeAsState()
            val saveResult by viewModel.saveResult.observeAsState()
            val updateResult by viewModel.updateResult.observeAsState()

            // LaunchedEffect (bukan pemanggilan langsung di body composable)
            // supaya Toast/finish() hanya jalan SEKALI saat sinyal berubah,
            // bukan berulang tiap recomposition (mis. tiap keystroke di
            // baris lain akan me-recompose seluruh layar ini).
            LaunchedEffect(errorMessage) {
                errorMessage?.let { message ->
                    Toast.makeText(this@JournalEntryActivity, message, Toast.LENGTH_LONG).show()
                    viewModel.clearError()
                }
            }
            LaunchedEffect(saveResult) {
                if (saveResult != null) {
                    Toast.makeText(this@JournalEntryActivity, "Entri tersimpan", Toast.LENGTH_SHORT).show()
                    finish()
                }
            }
            LaunchedEffect(updateResult) {
                if (updateResult == true) {
                    Toast.makeText(this@JournalEntryActivity, "Entri diperbarui", Toast.LENGTH_SHORT).show()
                    finish()
                }
            }

            val isEditable = !viewModel.isLocked

            AumoTheme {
                JournalEntryScreen(
                    pageTitle = if (entryId == null) "Journal Entry" else "Edit Journal Entry",
                    journalType = viewModel.journalType,
                    onJournalTypeChange = { viewModel.setJournalType(it) },
                    entryDate = viewModel.entryDate,
                    onEntryDateClick = { showDatePicker() },
                    transactionNumber = viewModel.transactionNumber,
                    isLocked = viewModel.isLocked,
                    isEditable = isEditable,
                    lines = viewModel.lines,
                    accounts = viewModel.accounts,
                    onAddLine = { viewModel.addLine() },
                    onRemoveLine = { viewModel.removeLine(it) },
                    totalDebitText = CurrencyFormatter.format(viewModel.totalDebit()),
                    totalCreditText = CurrencyFormatter.format(viewModel.totalCredit()),
                    isBalanced = viewModel.isBalanced(),
                    isEditingMode = entryId != null,
                    submitButtonText = if (entryId == null) "Simpan" else "Perbarui",
                    onCancel = { finish() },
                    onSubmit = { viewModel.save() }
                )
            }
        }
    }

    private fun showDatePicker() {
        val current = viewModel.entryDate
        DatePickerDialog(
            this,
            { _, year, month, day ->
                val updated = Calendar.getInstance().apply { set(year, month, day) }
                viewModel.setEntryDate(updated)
            },
            current.get(Calendar.YEAR),
            current.get(Calendar.MONTH),
            current.get(Calendar.DAY_OF_MONTH)
        ).show()
    }
}
