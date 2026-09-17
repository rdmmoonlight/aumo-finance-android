package com.aumofinance.app.home

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.runtime.getValue
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.lifecycleScope
import com.aumofinance.app.coa.CoaActivity
import com.aumofinance.app.dashboard.DashboardActivity
import com.aumofinance.app.data.DbConnectionManager
import com.aumofinance.app.journal.JournalEntryActivity
import com.aumofinance.app.periods.PeriodsActivity
import com.aumofinance.app.reports.journal.GeneralJournalReportActivity
import com.aumofinance.app.reports.menu.ReportsMenuActivity
import com.aumofinance.app.settings.SettingsActivity
import com.aumofinance.app.ui.theme.AumoTheme
import kotlinx.coroutines.launch

class HomeActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        // Memicu koneksi/ping ke DB tunggal (Render.com)
        initDatabaseConnection()

        setContent {
            // Mengambil status koneksi global dari Singleton
            val isDbConnected by DbConnectionManager.isDbConnected.collectAsStateWithLifecycle()

            AumoTheme {
                HomeScreen(
                    dashboard =
                        HomeMenuItem(
                            title = "Dashboard",
                            subtitle = "Ringkasan posisi keuangan periode berjalan",
                            icon = HomeIcons.Dashboard,
                            onClick = { open(DashboardActivity::class.java) },
                        ),
                    journalEntry =
                        HomeMenuItem(
                            title = "Journal Entry",
                            subtitle = "Catat transaksi baru",
                            icon = HomeIcons.JournalEntry,
                            onClick = { open(JournalEntryActivity::class.java) },
                        ),
                    generalJournal =
                        HomeMenuItem(
                            title = "General Journal",
                            subtitle = "Riwayat jurnal umum",
                            icon = HomeIcons.GeneralJournal,
                            onClick = { open(GeneralJournalReportActivity::class.java) },
                        ),
                    periods =
                        HomeMenuItem(
                            title = "Periode",
                            subtitle = "Kelola periode akuntansi",
                            icon = HomeIcons.Periods,
                            onClick = { open(PeriodsActivity::class.java) },
                        ),
                    coa =
                        HomeMenuItem(
                            title = "Chart of Accounts",
                            subtitle = "Daftar & kategori akun",
                            icon = HomeIcons.Coa,
                            onClick = { open(CoaActivity::class.java) },
                        ),
                    reports =
                        HomeMenuItem(
                            title = "Reports",
                            subtitle = "Buku besar, neraca saldo, laporan keuangan",
                            icon = HomeIcons.Reports,
                            onClick = { open(ReportsMenuActivity::class.java) },
                        ),
                    isDbConnected = isDbConnected,
                    onSettingsClick = { open(SettingsActivity::class.java) },
                )
            }
        }
    }

    private fun initDatabaseConnection() {
        lifecycleScope.launch {
            DbConnectionManager.ensureConnected {
                // Ganti dengan pemanggilan Retrofit / Ktor / Room / API Render.com kamu yang sebenarnya.
                // Contoh jika menggunakan API health check:
                // apiService.pingServer()
            }
        }
    }

    private fun open(activity: Class<*>) {
        startActivity(Intent(this, activity))
    }
}
