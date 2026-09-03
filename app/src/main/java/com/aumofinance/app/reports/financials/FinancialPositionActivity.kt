package com.aumofinance.app.reports.financials

import android.os.Bundle
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Statement of Financial Position (Neraca): Aset = Liabilitas + Ekuitas.
class FinancialPositionActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_financial_position)

        viewModel.financialPosition.observe(this) { render(it) }
        viewModel.loadFinancialPosition()
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadFinancialPosition()
    }

    private fun render(report: FinancialPositionReport?) {
        findViewById<TextView>(R.id.textPeriodName).text = report?.selectedPeriodName ?: "Belum ada periode dipilih"
        val container = findViewById<LinearLayout>(R.id.containerContent)
        container.removeAllViews()
        if (report == null) return

        container.addView(ReportRowBuilder.sectionTitle(this, "Aset"))
        report.assetAccounts.forEach { container.addView(ReportRowBuilder.row(this, it.accountName, it.amount, indent = true)) }
        container.addView(ReportRowBuilder.row(this, "Total Aset", report.totalAssets, bold = true))

        container.addView(ReportRowBuilder.sectionTitle(this, "Liabilitas"))
        report.liabilityAccounts.forEach { container.addView(ReportRowBuilder.row(this, it.accountName, it.amount, indent = true)) }
        container.addView(ReportRowBuilder.row(this, "Total Liabilitas", report.totalLiabilities, bold = true))

        container.addView(ReportRowBuilder.sectionTitle(this, "Ekuitas"))
        // Baris "Retained Earnings" sudah termasuk di akhir list ini dari backend.
        report.equityAccounts.forEach { container.addView(ReportRowBuilder.row(this, it.accountName, it.amount, indent = true)) }
        container.addView(ReportRowBuilder.row(this, "Total Ekuitas", report.totalEquity, bold = true))

        container.addView(ReportRowBuilder.divider(this))
        container.addView(ReportRowBuilder.row(this, "Total Liabilitas + Ekuitas", report.totalLiabilitiesAndEquity, bold = true))

        val badge = TextView(this).apply {
            text = if (report.isBalanced) "Neraca Balance" else "Neraca TIDAK Balance"
            setTextColor(if (report.isBalanced) 0xFF4FA36A.toInt() else 0xFFD7192F.toInt())
            setPadding(0, 12, 0, 0)
        }
        container.addView(badge)
    }
}
