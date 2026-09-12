package com.aumofinance.app.reports.ledger.permanent

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.reports.ledger.LedgerScreen
import com.aumofinance.app.ui.theme.AumoTheme

class GeneralLedgerPermanentActivity : ComponentActivity() {
    private val viewModel: GeneralLedgerPermanentViewModel by viewModels()

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
