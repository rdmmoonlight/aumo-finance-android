package com.aumofinance.app.reports.financials

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

class CashFlowActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_cash_flow)

        viewModel.cashFlow.observe(this) { /* bind Operasi/Investasi/Pendanaan + saldo kas akhir */ }
        viewModel.loadCashFlow()
    }
}
