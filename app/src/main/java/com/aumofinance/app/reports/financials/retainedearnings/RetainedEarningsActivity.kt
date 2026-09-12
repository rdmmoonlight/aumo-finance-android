package com.aumofinance.app.reports.financials.retainedearnings

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.ui.theme.AumoTheme

class RetainedEarningsActivity : ComponentActivity() {
    private val viewModel: RetainedEarningsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                RetainedEarningsScreen(report = viewModel.report)
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }
}
