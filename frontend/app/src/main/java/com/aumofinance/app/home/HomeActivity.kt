package com.aumofinance.app.home

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.aumofinance.app.coa.CoaActivity
import com.aumofinance.app.dashboard.DashboardActivity
import com.aumofinance.app.journal.JournalEntryActivity
import com.aumofinance.app.periods.PeriodsActivity
import com.aumofinance.app.reports.journal.GeneralJournalReportActivity
import com.aumofinance.app.reports.menu.ReportsMenuActivity
import com.aumofinance.app.settings.SettingsActivity
import com.aumofinance.app.ui.theme.AumoTheme

// Landing page pasca-login: hub navigasi ke seluruh fitur app. Ini SATU-
// SATUNYA layar tujuan setelah LoginActivity.
//
// Ditulis ulang dengan Jetpack Compose (sebelumnya RecyclerView + XML).
// Hanya 6 kotak yang ditampilkan di Home — 13 halaman laporan TIDAK
// dipisah satu-satu di sini, melainkan dikelompokkan di balik satu kotak
// "Reports" (lihat ReportsMenuActivity), kecuali General Journal yang
// sengaja punya kotak sendiri karena paling sering dipakai.
class HomeActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                HomeScreen(
                    dashboard = HomeMenuItem(
                        title = "Dashboard",
                        subtitle = "Ringkasan posisi keuangan periode berjalan",
                        icon = HomeIcons.Dashboard,
                        onClick = { open(DashboardActivity::class.java) }
                    ),
                    journalEntry = HomeMenuItem(
                        title = "Journal Entry",
                        subtitle = "Catat transaksi baru",
                        icon = HomeIcons.JournalEntry,
                        onClick = { open(JournalEntryActivity::class.java) }
                    ),
                    generalJournal = HomeMenuItem(
                        title = "General Journal",
                        subtitle = "Riwayat jurnal umum",
                        icon = HomeIcons.GeneralJournal,
                        onClick = { open(GeneralJournalReportActivity::class.java) }
                    ),
                    periods = HomeMenuItem(
                        title = "Periode",
                        subtitle = "Kelola periode akuntansi",
                        icon = HomeIcons.Periods,
                        onClick = { open(PeriodsActivity::class.java) }
                    ),
                    coa = HomeMenuItem(
                        title = "Chart of Accounts",
                        subtitle = "Daftar & kategori akun",
                        icon = HomeIcons.Coa,
                        onClick = { open(CoaActivity::class.java) }
                    ),
                    reports = HomeMenuItem(
                        title = "Reports",
                        subtitle = "Buku besar, neraca saldo, laporan keuangan",
                        icon = HomeIcons.Reports,
                        onClick = { open(ReportsMenuActivity::class.java) }
                    ),
                    onSettingsClick = { open(SettingsActivity::class.java) }
                )
            }
        }
    }

    private fun open(activity: Class<*>) {
        startActivity(Intent(this, activity))
    }
}
