package com.aumofinance.app.reports.journal

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Menampilkan seluruh entri (General + Adjusting) periode terpilih,
// dikelompokkan per tanggal, kolom Ref#/Akun/Debit/Kredit; baris kredit
// diindentasi satu tab dari debit.
class GeneralJournalReportActivity : AppCompatActivity() {
    private val viewModel: JournalReportViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_general_journal_report)

        // TODO: sinkronkan TopHeader.PeriodText dengan viewModel.selectedPeriodName
        viewModel.entries.observe(this) { /* bind ke RecyclerView berkelompok per tanggal */ }
        viewModel.loadGeneral()
    }
}
