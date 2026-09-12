package com.aumofinance.app.reports.financials.incomestatement

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.ui.theme.AumoTheme

class IncomeStatementActivity : ComponentActivity() {
    private val viewModel: IncomeStatementViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                IncomeStatementScreen(report = viewModel.report)
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }
}
