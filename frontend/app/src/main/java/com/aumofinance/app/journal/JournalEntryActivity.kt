package com.aumofinance.app.journal

import android.app.DatePickerDialog
import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.runtime.LaunchedEffect
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.ui.theme.AumoTheme
import java.util.Calendar

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
            val errorMessage = viewModel.errorMessage
            val saveResult = viewModel.saveResult
            val updateResult = viewModel.updateResult

            // LaunchedEffect (rather than calling directly in the composable body)
            // so that Toast/finish() only runs ONCE when the signal changes,
            // instead of repeating on every recomposition (e.g. every keystroke on
            // another line will recompose this entire screen).
            LaunchedEffect(errorMessage) {
                errorMessage?.let { message ->
                    Toast.makeText(this@JournalEntryActivity, message, Toast.LENGTH_LONG).show()
                    viewModel.clearError()
                }
            }
            LaunchedEffect(saveResult) {
                if (saveResult != null) {
                    Toast.makeText(this@JournalEntryActivity, "Entry saved", Toast.LENGTH_SHORT).show()
                    finish()
                }
            }
            LaunchedEffect(updateResult) {
                if (updateResult == true) {
                    Toast.makeText(this@JournalEntryActivity, "Entry updated", Toast.LENGTH_SHORT).show()
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
                    submitButtonText = if (entryId == null) "Save" else "Update",
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
