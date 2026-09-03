package com.aumofinance.app.reports.financials

import android.os.Bundle
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

class CashFlowActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_cash_flow)

        viewModel.cashFlow.observe(this) { render(it) }
        viewModel.loadCashFlow()
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadCashFlow()
    }

    private fun render(report: CashFlowReport?) {
        findViewById<TextView>(R.id.textPeriodName).text = report?.selectedPeriodName ?: "Belum ada periode dipilih"
        val container = findViewById<LinearLayout>(R.id.containerContent)
        container.removeAllViews()
        if (report == null) return

        container.addView(ReportRowBuilder.sectionTitle(this, "Aktivitas Operasi"))
        report.operatingActivities.forEach { container.addView(ReportRowBuilder.row(this, it.description, it.amount, indent = true)) }
        container.addView(ReportRowBuilder.row(this, "Kas Bersih dari Operasi", report.netCashFromOperating, bold = true))

        container.addView(ReportRowBuilder.sectionTitle(this, "Aktivitas Investasi"))
        report.investingActivities.forEach { container.addView(ReportRowBuilder.row(this, it.description, it.amount, indent = true)) }
        container.addView(ReportRowBuilder.row(this, "Kas Bersih dari Investasi", report.netCashFromInvesting, bold = true))

        container.addView(ReportRowBuilder.sectionTitle(this, "Aktivitas Pendanaan"))
        report.financingActivities.forEach { container.addView(ReportRowBuilder.row(this, it.description, it.amount, indent = true)) }
        container.addView(ReportRowBuilder.row(this, "Kas Bersih dari Pendanaan", report.netCashFromFinancing, bold = true))

        container.addView(ReportRowBuilder.divider(this))
        container.addView(ReportRowBuilder.row(this, "Perubahan Kas Bersih", report.netChangeInCash, bold = true))
        container.addView(ReportRowBuilder.row(this, "Saldo Kas Awal", report.beginningCash))
        container.addView(ReportRowBuilder.row(this, "Saldo Kas Akhir", report.endingCash, bold = true))
    }
}
