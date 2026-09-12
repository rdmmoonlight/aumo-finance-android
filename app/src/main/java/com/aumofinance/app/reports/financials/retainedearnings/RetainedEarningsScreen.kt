package com.aumofinance.app.reports.financials.retainedearnings

import androidx.compose.runtime.Composable
import com.aumofinance.app.reports.financials.FinancialReportScaffold
import com.aumofinance.app.reports.financials.ReportDivider
import com.aumofinance.app.reports.financials.ReportRow
import com.aumofinance.app.reports.financials.RetainedEarningsReport

/** Padanan Compose dari activity_retained_earnings.xml. */
@Composable
fun RetainedEarningsScreen(report: RetainedEarningsReport?) {
    FinancialReportScaffold(periodName = report?.selectedPeriodName) {
        if (report == null) return@FinancialReportScaffold

        ReportRow("Saldo Awal Laba Ditahan", report.beginningRetainedEarnings)
        ReportRow("Laba Bersih Periode Ini", report.netIncome)
        ReportRow("Prive / Dividen", -report.dividendsOrDraws)
        ReportDivider()
        ReportRow("Saldo Akhir Laba Ditahan", report.endingRetainedEarnings, bold = true)
    }
}
