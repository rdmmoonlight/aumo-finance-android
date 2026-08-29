package com.aumofinance.app.reports.financials

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Statement of Financial Position (Neraca): Aset = Liabilitas + Ekuitas.
class FinancialPositionActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_financial_position)

        viewModel.financialPosition.observe(this) { /* bind Aset / Liabilitas / Ekuitas + validasi balance */ }
        viewModel.loadFinancialPosition()
    }
}
