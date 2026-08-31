package com.aumofinance.app.dashboard

import android.os.Bundle
import android.view.View
import android.widget.TextView
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R

class DashboardActivity : AppCompatActivity() {
    private val viewModel: DashboardViewModel by viewModels()

    private lateinit var textPeriodName: TextView
    private lateinit var textPeriodClosedBadge: TextView
    private lateinit var textTotalAssets: TextView
    private lateinit var textTotalLiabilities: TextView
    private lateinit var textTotalEquity: TextView
    private lateinit var textNetIncome: TextView
    private lateinit var textRevenueExpense: TextView
    private lateinit var textTotalCash: TextView
    private lateinit var textTotalBank: TextView

    private val cashAdapter = CashAccountAdapter(emptyList())
    private val bankAdapter = CashAccountAdapter(emptyList())

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_dashboard)

        textPeriodName = findViewById(R.id.textPeriodName)
        textPeriodClosedBadge = findViewById(R.id.textPeriodClosedBadge)
        textTotalAssets = findViewById(R.id.textTotalAssets)
        textTotalLiabilities = findViewById(R.id.textTotalLiabilities)
        textTotalEquity = findViewById(R.id.textTotalEquity)
        textNetIncome = findViewById(R.id.textNetIncome)
        textRevenueExpense = findViewById(R.id.textRevenueExpense)
        textTotalCash = findViewById(R.id.textTotalCash)
        textTotalBank = findViewById(R.id.textTotalBank)

        findViewById<RecyclerView>(R.id.recyclerCashAccounts).apply {
            layoutManager = LinearLayoutManager(this@DashboardActivity)
            adapter = cashAdapter
        }
        findViewById<RecyclerView>(R.id.recyclerBankAccounts).apply {
            layoutManager = LinearLayoutManager(this@DashboardActivity)
            adapter = bankAdapter
        }

        viewModel.summary.observe(this) { summary ->
            if (summary == null || !summary.hasPeriodSelected) {
                textPeriodName.text = "Belum ada periode dipilih"
                textPeriodClosedBadge.visibility = View.GONE
                return@observe
            }

            textPeriodName.text = summary.selectedPeriodName ?: "-"
            textPeriodClosedBadge.visibility = if (summary.isPeriodClosed) View.VISIBLE else View.GONE

            textTotalAssets.text = CurrencyFormatter.format(summary.totalAssets)
            textTotalLiabilities.text = CurrencyFormatter.format(summary.totalLiabilities)
            textTotalEquity.text = CurrencyFormatter.format(summary.totalEquity)
            textNetIncome.text = CurrencyFormatter.format(summary.netIncome)
            textRevenueExpense.text = "Pendapatan ${CurrencyFormatter.format(summary.totalRevenue)}  •  " +
                "Beban ${CurrencyFormatter.format(summary.totalExpenses)}"

            cashAdapter.submitList(summary.cashAccounts)
            bankAdapter.submitList(summary.bankAccounts)
            textTotalCash.text = "Total Kas: ${CurrencyFormatter.format(summary.totalCashOnHand)}"
            textTotalBank.text = "Total Bank: ${CurrencyFormatter.format(summary.totalBankBalance)}"
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        // Refresh setiap kembali ke Dashboard — periode aktif bisa berubah
        // dari halaman Periods tanpa Activity ini di-recreate.
        viewModel.load()
    }
}
