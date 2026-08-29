package com.aumofinance.app.reports.trialbalance

import android.os.Bundle
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R

// Neraca Saldo Pasca-Penutupan: type="post-closing". Retained Earnings sudah
// termasuk efek Closing (walau baris Closing itu sendiri tidak pernah
// tersimpan sebagai entri jurnal — ini varian ketiga yang baru ditemukan
// saat membaca TrialBalanceControllers.cs asli, sebelumnya tidak ada di UI.
class PostClosingTrialBalanceActivity : AppCompatActivity() {
    private val viewModel: TrialBalanceViewModel by viewModels()
    private lateinit var adapter: TrialBalanceAdapter

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_trial_balance)

        adapter = TrialBalanceAdapter(emptyList())
        findViewById<RecyclerView>(R.id.recyclerRows).apply {
            layoutManager = LinearLayoutManager(this@PostClosingTrialBalanceActivity)
            adapter = this@PostClosingTrialBalanceActivity.adapter
        }

        viewModel.report.observe(this) { bindReport(it) }
        viewModel.load(type = "post-closing")
    }

    override fun onResume() {
        super.onResume()
        viewModel.load(type = "post-closing")
    }

    private fun bindReport(report: TrialBalanceReport?) {
        adapter.submitList(report?.rows ?: emptyList())
        findViewById<TextView>(R.id.textReportTitle).text = report?.reportTitle ?: "Neraca Saldo Pasca-Penutupan"
        findViewById<TextView>(R.id.textPeriodName).text = report?.selectedPeriodName ?: "Belum ada periode dipilih"
        findViewById<TextView>(R.id.textTotals).text =
            "Debit: ${CurrencyFormatter.format(report?.totalDebit ?: 0.0)}   Kredit: ${CurrencyFormatter.format(report?.totalCredit ?: 0.0)}"

        val badge = findViewById<TextView>(R.id.textBalancedBadge)
        val balanced = report?.isBalanced == true
        badge.text = if (balanced) "Balanced" else "Unbalanced"
        badge.setTextColor(resources.getColor(if (balanced) R.color.colorGood else R.color.colorBad, theme))
    }
}
