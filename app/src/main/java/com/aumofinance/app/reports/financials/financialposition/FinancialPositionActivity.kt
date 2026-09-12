package com.aumofinance.app.reports.financials.financialposition

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.ui.theme.AumoTheme

class FinancialPositionActivity : ComponentActivity() {
    private val viewModel: FinancialPositionViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                FinancialPositionScreen(report = viewModel.report)
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }
}
