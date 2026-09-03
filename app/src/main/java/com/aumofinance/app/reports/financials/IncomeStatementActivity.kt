package com.aumofinance.app.reports.financials

import android.os.Bundle
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Bagian "Other Income & Expenses" hanya ditampilkan jika berisi data
// (disembunyikan jika kosong). Operating Income ditampilkan terpisah dari
// Net Income — keduanya bisa berbeda kalau ada Other Income/Expense.
class IncomeStatementActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_income_statement)

        viewModel.incomeStatement.observe(this) { render(it) }
        viewModel.loadIncomeStatement()
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadIncomeStatement()
    }

    private fun render(report: IncomeStatementReport?) {
        findViewById<TextView>(R.id.textPeriodName).text = report?.selectedPeriodName ?: "Belum ada periode dipilih"
        val container = findViewById<LinearLayout>(R.id.containerContent)
        container.removeAllViews()
        if (report == null) return

        container.addView(ReportRowBuilder.sectionTitle(this, "Pendapatan"))
        report.revenueAccounts.forEach { container.addView(ReportRowBuilder.row(this, it.accountName, it.amount, indent = true)) }
        container.addView(ReportRowBuilder.row(this, "Total Pendapatan", report.totalRevenue, bold = true))

        container.addView(ReportRowBuilder.sectionTitle(this, "Beban"))
        report.expenseAccounts.forEach { container.addView(ReportRowBuilder.row(this, it.accountName, it.amount, indent = true)) }
        container.addView(ReportRowBuilder.row(this, "Total Beban", report.totalExpenses, bold = true))

        container.addView(ReportRowBuilder.divider(this))
        container.addView(ReportRowBuilder.row(this, "Laba Operasional", report.operatingIncome, bold = true))

        if (report.otherIncomeAccounts.isNotEmpty()) {
            container.addView(ReportRowBuilder.sectionTitle(this, "Pendapatan Lain-lain"))
            report.otherIncomeAccounts.forEach { container.addView(ReportRowBuilder.row(this, it.accountName, it.amount, indent = true)) }
            container.addView(ReportRowBuilder.row(this, "Total Pendapatan Lain-lain", report.totalOtherIncome, bold = true))
        }

        if (report.otherExpenseAccounts.isNotEmpty()) {
            container.addView(ReportRowBuilder.sectionTitle(this, "Beban Lain-lain"))
            report.otherExpenseAccounts.forEach { container.addView(ReportRowBuilder.row(this, it.accountName, it.amount, indent = true)) }
            container.addView(ReportRowBuilder.row(this, "Total Beban Lain-lain", report.totalOtherExpenses, bold = true))
        }

        container.addView(ReportRowBuilder.divider(this))
        container.addView(ReportRowBuilder.row(this, "Laba Bersih", report.netIncome, bold = true))
    }
}
