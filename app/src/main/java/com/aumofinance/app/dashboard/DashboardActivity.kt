package com.aumofinance.app.dashboard

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import com.aumofinance.app.ui.theme.AumoTheme

class DashboardActivity : ComponentActivity() {
    private val viewModel: DashboardViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                DashboardScreen(summary = viewModel.summary)
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        // Refresh every time returning to Dashboard — active period might change
        // from the Periods page without this Activity being recreated.
        viewModel.load()
    }
}
