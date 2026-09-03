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

// Contents of the "Reports" box on Home. General Journal is NOT included
// here because it has its own dedicated box on Home (see HomeActivity).
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
            title = "General Ledger",
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
            title = "Trial Balance & Adjustments",
            items = listOf(
                ReportMenuItem("Trial Balance", ReportMenuIcons.TrialBalance) {
                    open(TrialBalanceActivity::class.java)
                },
                ReportMenuItem("Adjusting Journal", ReportMenuIcons.AdjustingJournal) {
                    open(AdjustingJournalReportActivity::class.java)
                },
                ReportMenuItem("Adjusted Trial Balance", ReportMenuIcons.TrialBalance) {
                    open(AdjustedTrialBalanceActivity::class.java)
                }
            )
        ),
        ReportMenuSection(
            title = "Worksheet",
            items = listOf(
                ReportMenuItem("Worksheet", ReportMenuIcons.Worksheet) {
                    open(WorksheetActivity::class.java)
                }
            )
        ),
        ReportMenuSection(
            title = "Financial Statements",
            items = listOf(
                ReportMenuItem("Income Statement", ReportMenuIcons.IncomeStatement) {
                    open(IncomeStatementActivity::class.java)
                },
                ReportMenuItem("Retained Earnings Statement", ReportMenuIcons.RetainedEarnings) {
                    open(RetainedEarningsActivity::class.java)
                },
                ReportMenuItem("Statement of Financial Position", ReportMenuIcons.FinancialPosition) {
                    open(FinancialPositionActivity::class.java)
                },
                ReportMenuItem("Statement of Cash Flows", ReportMenuIcons.CashFlow) {
                    open(CashFlowActivity::class.java)
                }
            )
        ),
        ReportMenuSection(
            title = "Closing",
            items = listOf(
                ReportMenuItem("Closing Journal", ReportMenuIcons.ClosingJournal) {
                    open(ClosingJournalActivity::class.java)
                },
                ReportMenuItem("Post-Closing Trial Balance", ReportMenuIcons.TrialBalance) {
                    open(PostClosingTrialBalanceActivity::class.java)
                }
            )
        )
    )

    private fun open(activity: Class<*>) {
        startActivity(Intent(this, activity))
    }
}
