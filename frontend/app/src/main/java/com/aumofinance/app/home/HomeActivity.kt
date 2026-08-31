package com.aumofinance.app.home

import android.content.Intent
import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.coa.CoaActivity
import com.aumofinance.app.dashboard.DashboardActivity
import com.aumofinance.app.journal.JournalEntryActivity
import com.aumofinance.app.periods.PeriodsActivity
import com.aumofinance.app.reports.financials.CashFlowActivity
import com.aumofinance.app.reports.financials.ClosingJournalActivity
import com.aumofinance.app.reports.financials.FinancialPositionActivity
import com.aumofinance.app.reports.financials.IncomeStatementActivity
import com.aumofinance.app.reports.financials.RetainedEarningsActivity
import com.aumofinance.app.reports.journal.AdjustingJournalReportActivity
import com.aumofinance.app.reports.journal.GeneralJournalReportActivity
import com.aumofinance.app.reports.ledger.GeneralLedgerPermanentActivity
import com.aumofinance.app.reports.ledger.GeneralLedgerTemporaryActivity
import com.aumofinance.app.reports.trialbalance.AdjustedTrialBalanceActivity
import com.aumofinance.app.reports.trialbalance.PostClosingTrialBalanceActivity
import com.aumofinance.app.reports.trialbalance.TrialBalanceActivity
import com.aumofinance.app.reports.worksheet.WorksheetActivity
import com.aumofinance.app.settings.SettingsActivity
import com.aumofinance.app.R

// Landing page pasca-login: hub navigasi ke seluruh fitur app. Ini SATU-
// SATUNYA layar tujuan setelah LoginActivity (MainActivity placeholder lama
// sudah dihapus — dulu LoginActivity malah mengarah ke situ, bukan ke sini,
// jadi menu ini tidak akan pernah tercapai walau sudah diisi; itu penyebab
// app "buntu" setelah login sebelum perbaikan ini).
class HomeActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_home)

        findViewById<RecyclerView>(R.id.recyclerMainMenu).apply {
            layoutManager = LinearLayoutManager(this@HomeActivity)
            adapter = MenuAdapter(mainMenuItems())
        }

        findViewById<RecyclerView>(R.id.recyclerReportsMenu).apply {
            layoutManager = LinearLayoutManager(this@HomeActivity)
            adapter = MenuAdapter(reportsMenuItems())
        }

        findViewById<android.widget.ImageButton>(R.id.buttonSettings).setOnClickListener {
            open(SettingsActivity::class.java)
        }
    }

    private fun mainMenuItems() = listOf(
        MenuItem("Dashboard") { open(DashboardActivity::class.java) },
        MenuItem("Periode") { open(PeriodsActivity::class.java) },
        MenuItem("Chart of Accounts") { open(CoaActivity::class.java) },
        MenuItem("Tambah Journal Entry") { open(JournalEntryActivity::class.java) }
    )

    private fun reportsMenuItems() = listOf(
        MenuItem("General Journal") { open(GeneralJournalReportActivity::class.java) },
        MenuItem("Adjusting Journal") { open(AdjustingJournalReportActivity::class.java) },
        MenuItem("General Ledger — Permanent") { open(GeneralLedgerPermanentActivity::class.java) },
        MenuItem("General Ledger — Temporary") { open(GeneralLedgerTemporaryActivity::class.java) },
        MenuItem("Trial Balance") { open(TrialBalanceActivity::class.java) },
        MenuItem("Adjusted Trial Balance") { open(AdjustedTrialBalanceActivity::class.java) },
        MenuItem("Post-Closing Trial Balance") { open(PostClosingTrialBalanceActivity::class.java) },
        MenuItem("Worksheet") { open(WorksheetActivity::class.java) },
        MenuItem("Income Statement") { open(IncomeStatementActivity::class.java) },
        MenuItem("Retained Earnings") { open(RetainedEarningsActivity::class.java) },
        MenuItem("Statement of Financial Position") { open(FinancialPositionActivity::class.java) },
        MenuItem("Cash Flow") { open(CashFlowActivity::class.java) },
        MenuItem("Closing Journal") { open(ClosingJournalActivity::class.java) }
    )

    private fun open(activity: Class<*>) {
        startActivity(Intent(this, activity))
    }
}
