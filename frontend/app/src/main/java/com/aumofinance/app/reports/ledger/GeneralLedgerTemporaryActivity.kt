package com.aumofinance.app.reports.ledger

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Akun Temporary (Laba Rugi): OperatingIncome, OperatingExpenses, OtherIncome,
// OtherExpenses. isTemporary=true.
class GeneralLedgerTemporaryActivity : AppCompatActivity() {
    private val viewModel: LedgerViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_general_ledger)

        viewModel.report.observe(this) { /* bind ke RecyclerView per akun, T-account style */ }
        viewModel.load(isTemporary = true)
    }
}
