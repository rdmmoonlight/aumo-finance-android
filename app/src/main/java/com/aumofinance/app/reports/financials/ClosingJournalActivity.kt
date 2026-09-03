package com.aumofinance.app.reports.financials

import android.os.Bundle
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R

// Read-only: entri Closing bersifat system-generated (dihitung on-the-fly
// dari Trial Balance oleh backend, TIDAK PERNAH tersimpan sebagai entri
// jurnal sungguhan) — tidak ada tombol tambah/edit/hapus di halaman ini.
class ClosingJournalActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_closing_journal)

        viewModel.closingJournal.observe(this) { render(it) }
        viewModel.loadClosingJournal()
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadClosingJournal()
    }

    private fun render(report: ClosingJournalReport?) {
        findViewById<TextView>(R.id.textPeriodName).text = report?.selectedPeriodName ?: "Belum ada periode dipilih"
        val data = report?.closingJournal
        findViewById<TextView>(R.id.textNetIncome).text =
            "Laba Bersih (ditutup ke ${data?.retainedEarningsAccountName ?: "Retained Earnings"}): ${CurrencyFormatter.format(data?.netIncome ?: 0.0)}"

        val container = findViewById<LinearLayout>(R.id.containerGroups)
        container.removeAllViews()

        data?.groups?.forEach { group ->
            container.addView(ReportRowBuilder.sectionTitle(this, group.description))
            group.lines.forEach { line ->
                val amount = if (line.debit > 0) line.debit else -line.credit
                val label = "${line.referenceNumber} - ${line.accountName}"
                container.addView(ReportRowBuilder.row(this, label, amount, indent = true))
            }
            container.addView(ReportRowBuilder.row(this, "Total", group.totalDebit, bold = true))
            container.addView(ReportRowBuilder.divider(this))
        }
    }
}
