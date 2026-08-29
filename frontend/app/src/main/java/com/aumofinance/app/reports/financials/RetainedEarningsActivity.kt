package com.aumofinance.app.reports.financials

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

class RetainedEarningsActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_retained_earnings)

        viewModel.retainedEarnings.observe(this) { /* bind Saldo Awal + Laba Bersih - Prive = Saldo Akhir */ }
        viewModel.loadRetainedEarnings()
    }
}
