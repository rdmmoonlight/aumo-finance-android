package com.aumofinance.app.reports.ledger.temporary

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.reports.ledger.LedgerScreen
import com.aumofinance.app.ui.theme.AumoTheme

class GeneralLedgerTemporaryActivity : ComponentActivity() {
    private val viewModel: GeneralLedgerTemporaryViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                LedgerScreen(report = viewModel.report)
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }
}
