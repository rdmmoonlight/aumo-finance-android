package com.aumofinance.app.reports.journal

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Sama seperti General Journal, tapi backend sudah memfilter journalType
// "Adjusting" saja; selalu menampilkan tombol edit/delete (tanpa mode toggle
// Edit terpisah seperti General Journal).
class AdjustingJournalReportActivity : AppCompatActivity() {
    private val viewModel: JournalReportViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_adjusting_journal_report)

        viewModel.entries.observe(this) { /* bind ke RecyclerView berkelompok per tanggal */ }
        viewModel.loadAdjusting()
    }
}
