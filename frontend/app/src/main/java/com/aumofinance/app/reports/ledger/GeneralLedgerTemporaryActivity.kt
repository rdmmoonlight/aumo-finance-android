package com.aumofinance.app.reports.ledger

import android.os.Bundle
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.R

// Akun Temporary (Laba Rugi): OperatingIncome, OperatingExpenses, OtherIncome,
// OtherExpenses. isTemporary=true.
class GeneralLedgerTemporaryActivity : AppCompatActivity() {
    private val viewModel: LedgerViewModel by viewModels()
    private lateinit var adapter: LedgerAdapter

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_general_ledger)

        adapter = LedgerAdapter(emptyList())
        findViewById<RecyclerView>(R.id.recyclerAccounts).apply {
            layoutManager = LinearLayoutManager(this@GeneralLedgerTemporaryActivity)
            adapter = this@GeneralLedgerTemporaryActivity.adapter
        }

        viewModel.report.observe(this) { report ->
            adapter.submitList(report?.ledgers ?: emptyList())
            findViewById<TextView>(R.id.textPeriodName).text = report?.selectedPeriodName ?: "Belum ada periode dipilih"
        }
        viewModel.load(isTemporary = true)
    }

    override fun onResume() {
        super.onResume()
        viewModel.load(isTemporary = true)
    }
}
