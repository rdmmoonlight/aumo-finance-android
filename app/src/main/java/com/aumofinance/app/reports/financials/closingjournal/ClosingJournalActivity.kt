package com.aumofinance.app.reports.financials.closingjournal

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.ui.theme.AumoTheme

class ClosingJournalActivity : ComponentActivity() {
    private val viewModel: ClosingJournalViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                ClosingJournalScreen(report = viewModel.report)
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }
}
