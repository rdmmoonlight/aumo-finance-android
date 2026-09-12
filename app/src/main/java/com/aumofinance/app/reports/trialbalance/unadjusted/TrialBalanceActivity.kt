package com.aumofinance.app.reports.trialbalance.unadjusted

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.reports.trialbalance.TrialBalanceScreen
import com.aumofinance.app.ui.theme.AumoTheme

class TrialBalanceActivity : ComponentActivity() {
    private val viewModel: TrialBalanceViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                TrialBalanceScreen(report = viewModel.report, fallbackTitle = "Neraca Saldo")
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }
}
