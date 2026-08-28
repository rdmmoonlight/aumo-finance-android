package com.aumofinance.app.reports.financials

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Read-only: entri Closing bersifat system-generated, tidak ada tombol tambah/edit/hapus di sini.
class ClosingJournalActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_closing_journal)

        viewModel.closingJournal.observe(this) { /* bind daftar entri debit/kredit read-only */ }
        viewModel.loadClosingJournal(periodId = 0)
    }
}
