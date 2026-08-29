package com.aumofinance.app.reports.financials

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Bagian "Other Income & Expenses" hanya ditampilkan jika berisi data
// (disembunyikan jika kosong, sama seperti halaman web). Operating Income
// ditampilkan terpisah dari Net Income, tidak boleh sama nilainya.
class IncomeStatementActivity : AppCompatActivity() {
    private val viewModel: FinancialsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_income_statement)

        viewModel.incomeStatement.observe(this) { /* bind Revenue/Expenses/Operating Income/Other/Net Income */ }
        viewModel.loadIncomeStatement()
    }
}
