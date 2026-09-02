package com.aumofinance.app.reports.menu

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.aumofinance.app.reports.financials.CashFlowActivity
import com.aumofinance.app.reports.financials.ClosingJournalActivity
import com.aumofinance.app.reports.financials.FinancialPositionActivity
import com.aumofinance.app.reports.financials.IncomeStatementActivity
import com.aumofinance.app.reports.financials.RetainedEarningsActivity
import com.aumofinance.app.reports.journal.AdjustingJournalReportActivity
import com.aumofinance.app.reports.ledger.GeneralLedgerPermanentActivity
import com.aumofinance.app.reports.ledger.GeneralLedgerTemporaryActivity
import com.aumofinance.app.reports.trialbalance.AdjustedTrialBalanceActivity
import com.aumofinance.app.reports.trialbalance.PostClosingTrialBalanceActivity
import com.aumofinance.app.reports.trialbalance.TrialBalanceActivity
import com.aumofinance.app.reports.worksheet.WorksheetActivity
import com.aumofinance.app.ui.theme.AumoTheme

// Isi dari kotak "Reports" di Home. General Journal TIDAK dimasukkan di
// sini karena sudah punya kotak sendiri di Home (lihat HomeActivity).
class ReportsMenuActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            AumoTheme {
                ReportsMenuScreen(
                    sections = buildSections(),
                    onBackClick = { finish() }
                )
            }
        }
    }

    private fun buildSections(): List<ReportMenuSection> = listOf(
        ReportMenuSection(
            title = "Jurnal",
            items = listOf(
                ReportMenuItem("Adjusting Journal", ReportMenuIcons.AdjustingJournal) {
                    open(AdjustingJournalReportActivity::class.java)
                }
            )
        ),
        ReportMenuSection(
            title = "Buku Besar",
            items = listOf(
                ReportMenuItem("General Ledger — Permanent", ReportMenuIcons.LedgerPermanent) {
                    open(GeneralLedgerPermanentActivity::class.java)
                },
                ReportMenuItem("General Ledger — Temporary", ReportMenuIcons.LedgerTemporary) {
                    open(GeneralLedgerTemporaryActivity::class.java)
                }
            )
        ),
        ReportMenuSection(
            title = "Neraca Saldo",
            items = listOf(
                ReportMenuItem("Trial Balance", ReportMenuIcons.TrialBalance) {
                    open(TrialBalanceActivity::class.java)
                },
                ReportMenuItem("Adjusted Trial Balance", ReportMenuIcons.TrialBalance) {
                    open(AdjustedTrialBalanceActivity::class.java)
                },
                ReportMenuItem("Post-Closing Trial Balance", ReportMenuIcons.TrialBalance) {
                    open(PostClosingTrialBalanceActivity::class.java)
                }
            )
        ),
        ReportMenuSection(
            title = "Worksheet & Penutupan",
            items = listOf(
                ReportMenuItem("Worksheet", ReportMenuIcons.Worksheet) {
                    open(WorksheetActivity::class.java)
                },
                ReportMenuItem("Closing Journal", ReportMenuIcons.ClosingJournal) {
                    open(ClosingJournalActivity::class.java)
                }
            )
        ),
        ReportMenuSection(
            title = "Laporan Keuangan",
            items = listOf(
                ReportMenuItem("Income Statement", ReportMenuIcons.IncomeStatement) {
                    open(IncomeStatementActivity::class.java)
                },
                ReportMenuItem("Retained Earnings", ReportMenuIcons.RetainedEarnings) {
                    open(RetainedEarningsActivity::class.java)
                },
                ReportMenuItem("Statement of Financial Position", ReportMenuIcons.FinancialPosition) {
                    open(FinancialPositionActivity::class.java)
                },
                ReportMenuItem("Cash Flow", ReportMenuIcons.CashFlow) {
                    open(CashFlowActivity::class.java)
                }
            )
        )
    )

    private fun open(activity: Class<*>) {
        startActivity(Intent(this, activity))
    }
}
