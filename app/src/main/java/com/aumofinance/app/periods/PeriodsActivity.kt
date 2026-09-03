package com.aumofinance.app.periods

import android.app.AlertDialog
import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.runtime.LaunchedEffect
import com.aumofinance.app.ui.theme.AumoTheme

// Host Compose tipis — semua tampilan ada di PeriodsScreen.kt / OpenPeriodDialog,
// state ada di PeriodsViewModel. Sebelumnya berbasis RecyclerView + AlertDialog
// View/XML biasa; dipindah ke Jetpack Compose menyusul Journal Entry & Home.
class PeriodsActivity : ComponentActivity() {
    private val viewModel: PeriodsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        viewModel.load()

        setContent {
            val toastMessage = viewModel.toastMessage

            // LaunchedEffect supaya Toast hanya muncul SEKALI saat pesan berubah,
            // bukan berulang setiap recomposition.
            LaunchedEffect(toastMessage) {
                toastMessage?.let { message ->
                    Toast.makeText(this@PeriodsActivity, message, Toast.LENGTH_LONG).show()
                    viewModel.clearToast()
                }
            }

            AumoTheme {
                PeriodsScreen(
                    periods = viewModel.periods,
                    selectedPeriodId = viewModel.selectedPeriodId,
                    onSelect = { period -> viewModel.select(period.id) },
                    onCloseRequest = { period -> confirmClose(period) },
                    onOpenNewPeriodClick = { viewModel.openNewPeriodDialog() }
                )

                viewModel.openPeriodInfo?.let { info ->
                    OpenPeriodDialog(
                        info = info,
                        onDismiss = { viewModel.dismissOpenPeriodDialog() },
                        onSubmit = { request -> viewModel.open(request) }
                    )
                }
            }
        }
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }

    // Dialog konfirmasi native (bukan Compose) sudah cukup untuk aksi
    // sekali-tap sederhana seperti ini — tidak perlu jadi bagian dari
    // PeriodsScreen composable.
    private fun confirmClose(period: Period) {
        AlertDialog.Builder(this)
            .setTitle("Close Period?")
            .setMessage("Period \"${period.periodName}\" will be closed and cannot accept new entries anymore. Continue?")
            .setPositiveButton("Close") { _, _ -> viewModel.close(period.id) }
            .setNegativeButton("Cancel", null)
            .show()
    }
}
