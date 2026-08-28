package com.aumofinance.app.reports.trialbalance

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Neraca Saldo (belum disesuaikan): hanya menghitung jurnal type=General.
class TrialBalanceActivity : AppCompatActivity() {
    private val viewModel: TrialBalanceViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_trial_balance)

        viewModel.report.observe(this) { /* bind ke tabel + footer total debit/kredit */ }
        viewModel.load(periodId = 0, adjusted = false)
    }
}
