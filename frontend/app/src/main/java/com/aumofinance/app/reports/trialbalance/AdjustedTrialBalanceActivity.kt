package com.aumofinance.app.reports.trialbalance

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Neraca Saldo Disesuaikan: menghitung jurnal type=General + Adjusting.
// PENTING: halaman ini dan TrialBalanceActivity berbagi Activity/route param
// di versi web sebelumnya, tapi keduanya HARUS memuat ulang data saat
// parameter berubah (bukan hanya sekali di awal) — riwayat bug: data
// Adjusted TB pernah identik dengan Unadjusted TB karena reload tidak terjadi.
class AdjustedTrialBalanceActivity : AppCompatActivity() {
    private val viewModel: TrialBalanceViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_trial_balance)

        viewModel.report.observe(this) { /* bind ke tabel + footer total debit/kredit */ }
        viewModel.load(periodId = 0, adjusted = true)
    }
}
