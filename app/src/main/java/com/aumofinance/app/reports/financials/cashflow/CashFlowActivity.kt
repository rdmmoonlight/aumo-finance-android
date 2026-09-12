package com.aumofinance.app.reports.financials.cashflow

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.ui.theme.AumoTheme

class CashFlowActivity : ComponentActivity() {
    private val viewModel: CashFlowViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                CashFlowScreen(report = viewModel.report)
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }
}
