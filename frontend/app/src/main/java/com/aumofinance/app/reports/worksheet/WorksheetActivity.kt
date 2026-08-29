package com.aumofinance.app.reports.worksheet

import android.os.Bundle
import android.view.Gravity
import android.view.ViewGroup
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R

// Footer worksheet WAJIB menampilkan 3 baris total standar akuntansi:
// 1) Total (sebelum plug), 2) Laba/Rugi Bersih (plug ke Neraca), 3) Total Akhir
// (setelah plug). Istilah "plug" tidak ditampilkan ke pengguna — pakai bahasa
// Indonesia biasa. Tabel 10 kolom angka terlalu lebar untuk layar ponsel,
// jadi dibungkus HorizontalScrollView; baris dibangun programatik (bukan
// RecyclerView) karena jumlah akun biasanya kecil dan kolomnya tetap sama
// untuk semua baris.
class WorksheetActivity : AppCompatActivity() {
    private val viewModel: WorksheetViewModel by viewModels()

    private val columnHeaders = listOf(
        "Akun", "TB Debit", "TB Kredit", "Adj Debit", "Adj Kredit",
        "Adj.TB Debit", "Adj.TB Kredit", "L/R Debit", "L/R Kredit", "Neraca Debit", "Neraca Kredit"
    )

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_worksheet)

        viewModel.report.observe(this) { report -> renderTable(report) }
        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }

    private fun renderTable(report: WorksheetReport?) {
        findViewById<TextView>(R.id.textPeriodName).text = report?.selectedPeriodName ?: "Belum ada periode dipilih"

        val container = findViewById<LinearLayout>(R.id.containerTable)
        container.removeAllViews()

        container.addView(buildRow(columnHeaders, isHeader = true))

        report?.rows?.forEach { row ->
            val cells = listOf(
                row.accountName,
                fmt(row.tbDebit), fmt(row.tbCredit),
                fmt(row.adjDebit), fmt(row.adjCredit),
                fmt(row.adjTbDebit), fmt(row.adjTbCredit),
                fmt(row.isDebit), fmt(row.isCredit),
                fmt(row.bsDebit), fmt(row.bsCredit)
            )
            container.addView(buildRow(cells, isHeader = false))
        }

        report?.totals?.let { totals ->
            container.addView(buildRow(
                listOf("Total", fmt(totals.tbDebit), fmt(totals.tbCredit), fmt(totals.adjDebit), fmt(totals.adjCredit),
                    fmt(totals.adjTbDebit), fmt(totals.adjTbCredit), fmt(totals.isDebit), fmt(totals.isCredit),
                    fmt(totals.bsDebit), fmt(totals.bsCredit)),
                isHeader = true
            ))

            val netIncome = totals.netIncome
            val isPositive = netIncome >= 0
            // Plug: Laba Bersih menambah sisi Debit L/R (menyeimbangkan L/R yang lebih besar
            // di Kredit) dan sisi Kredit Neraca (menyeimbangkan Neraca yang lebih besar di Debit).
            container.addView(buildRow(
                listOf(
                    if (isPositive) "Laba Bersih" else "Rugi Bersih",
                    "", "", "", "", "", "",
                    if (isPositive) fmt(netIncome) else "", if (!isPositive) fmt(-netIncome) else "",
                    if (!isPositive) fmt(-netIncome) else "", if (isPositive) fmt(netIncome) else ""
                ),
                isHeader = false
            ))

            container.addView(buildRow(
                listOf(
                    "Total Akhir",
                    "", "", "", "",
                    fmt(totals.adjTbDebit), fmt(totals.adjTbCredit),
                    fmt(totals.isDebit + (if (isPositive) netIncome else 0.0)),
                    fmt(totals.isCredit + (if (!isPositive) -netIncome else 0.0)),
                    fmt(totals.bsDebit + (if (!isPositive) -netIncome else 0.0)),
                    fmt(totals.bsCredit + (if (isPositive) netIncome else 0.0))
                ),
                isHeader = true
            ))
        }
    }

    private fun fmt(value: Double): String = if (value == 0.0) "" else CurrencyFormatter.format(value)

    private fun buildRow(cells: List<String>, isHeader: Boolean): LinearLayout {
        val density = resources.displayMetrics.density
        val row = LinearLayout(this).apply {
            orientation = LinearLayout.HORIZONTAL
            layoutParams = ViewGroup.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT)
        }
        cells.forEachIndexed { index, text ->
            val widthDp = if (index == 0) 140 else 110
            val cell = TextView(this).apply {
                this.text = text
                setPadding((12 * density).toInt(), (8 * density).toInt(), (12 * density).toInt(), (8 * density).toInt())
                textSize = 12f
                gravity = if (index == 0) Gravity.START else Gravity.END
                setTextColor(resources.getColor(if (isHeader) R.color.colorPrimary else android.R.color.white, theme))
                layoutParams = LinearLayout.LayoutParams(
                    (widthDp * density).toInt(),
                    ViewGroup.LayoutParams.WRAP_CONTENT
                )
            }
            row.addView(cell)
        }
        return row
    }
}
