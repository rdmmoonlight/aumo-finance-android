package com.aumofinance.app.reports.financials.cashflow

import androidx.compose.runtime.Composable
import com.aumofinance.app.reports.financials.CashFlowReport
import com.aumofinance.app.reports.financials.FinancialReportScaffold
import com.aumofinance.app.reports.financials.ReportDivider
import com.aumofinance.app.reports.financials.ReportRow
import com.aumofinance.app.reports.financials.ReportSectionTitle

/** Padanan Compose dari activity_cash_flow.xml. */
@Composable
fun CashFlowScreen(report: CashFlowReport?) {
    FinancialReportScaffold(periodName = report?.selectedPeriodName) {
        if (report == null) return@FinancialReportScaffold

        ReportSectionTitle("Aktivitas Operasi")
        report.operatingActivities.forEach { ReportRow(it.description, it.amount, indent = true) }
        ReportRow("Kas Bersih dari Operasi", report.netCashFromOperating, bold = true)

        ReportSectionTitle("Aktivitas Investasi")
        report.investingActivities.forEach { ReportRow(it.description, it.amount, indent = true) }
        ReportRow("Kas Bersih dari Investasi", report.netCashFromInvesting, bold = true)

        ReportSectionTitle("Aktivitas Pendanaan")
        report.financingActivities.forEach { ReportRow(it.description, it.amount, indent = true) }
        ReportRow("Kas Bersih dari Pendanaan", report.netCashFromFinancing, bold = true)

        ReportDivider()
        ReportRow("Perubahan Kas Bersih", report.netChangeInCash, bold = true)
        ReportRow("Saldo Kas Awal", report.beginningCash)
        ReportRow("Saldo Kas Akhir", report.endingCash, bold = true)
    }
}
