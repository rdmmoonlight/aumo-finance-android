package com.aumofinance.app.reports.trialbalance.adjusted

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.reports.trialbalance.TrialBalanceScreen
import com.aumofinance.app.ui.theme.AumoTheme

class AdjustedTrialBalanceActivity : ComponentActivity() {
    private val viewModel: AdjustedTrialBalanceViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                TrialBalanceScreen(report = viewModel.report, fallbackTitle = "Neraca Saldo Disesuaikan")
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }
}
