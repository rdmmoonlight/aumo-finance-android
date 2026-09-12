package com.aumofinance.app.reports.financials.incomestatement

import androidx.compose.runtime.Composable
import com.aumofinance.app.reports.financials.FinancialReportScaffold
import com.aumofinance.app.reports.financials.IncomeStatementReport
import com.aumofinance.app.reports.financials.ReportDivider
import com.aumofinance.app.reports.financials.ReportRow
import com.aumofinance.app.reports.financials.ReportSectionTitle

/**
 * Bagian "Other Income & Expenses" hanya ditampilkan jika berisi data
 * (disembunyikan jika kosong). Operating Income ditampilkan terpisah dari
 * Net Income — keduanya bisa berbeda kalau ada Other Income/Expense.
 * Padanan Compose dari activity_income_statement.xml.
 */
@Composable
fun IncomeStatementScreen(report: IncomeStatementReport?) {
    FinancialReportScaffold(periodName = report?.selectedPeriodName) {
        if (report == null) return@FinancialReportScaffold

        ReportSectionTitle("Pendapatan")
        report.revenueAccounts.forEach { ReportRow(it.accountName, it.amount, indent = true) }
        ReportRow("Total Pendapatan", report.totalRevenue, bold = true)

        ReportSectionTitle("Beban")
        report.expenseAccounts.forEach { ReportRow(it.accountName, it.amount, indent = true) }
        ReportRow("Total Beban", report.totalExpenses, bold = true)

        ReportDivider()
        ReportRow("Laba Operasional", report.operatingIncome, bold = true)

        if (report.otherIncomeAccounts.isNotEmpty()) {
            ReportSectionTitle("Pendapatan Lain-lain")
            report.otherIncomeAccounts.forEach { ReportRow(it.accountName, it.amount, indent = true) }
            ReportRow("Total Pendapatan Lain-lain", report.totalOtherIncome, bold = true)
        }

        if (report.otherExpenseAccounts.isNotEmpty()) {
            ReportSectionTitle("Beban Lain-lain")
            report.otherExpenseAccounts.forEach { ReportRow(it.accountName, it.amount, indent = true) }
            ReportRow("Total Beban Lain-lain", report.totalOtherExpenses, bold = true)
        }

        ReportDivider()
        ReportRow("Laba Bersih", report.netIncome, bold = true)
    }
}
