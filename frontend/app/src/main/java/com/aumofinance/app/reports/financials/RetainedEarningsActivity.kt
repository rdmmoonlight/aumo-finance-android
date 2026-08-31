package com.aumofinance.app.reports.financials

import android.os.Bundle
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

class RetainedEarningsActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_retained_earnings)

        viewModel.retainedEarnings.observe(this) { render(it) }
        viewModel.loadRetainedEarnings()
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadRetainedEarnings()
    }

    private fun render(report: RetainedEarningsReport?) {
        findViewById<TextView>(R.id.textPeriodName).text = report?.selectedPeriodName ?: "Belum ada periode dipilih"
        val container = findViewById<LinearLayout>(R.id.containerContent)
        container.removeAllViews()
        if (report == null) return

        container.addView(ReportRowBuilder.row(this, "Saldo Awal Laba Ditahan", report.beginningRetainedEarnings))
        container.addView(ReportRowBuilder.row(this, "Laba Bersih Periode Ini", report.netIncome))
        container.addView(ReportRowBuilder.row(this, "Prive / Dividen", -report.dividendsOrDraws))
        container.addView(ReportRowBuilder.divider(this))
        container.addView(ReportRowBuilder.row(this, "Saldo Akhir Laba Ditahan", report.endingRetainedEarnings, bold = true))
    }
}
