package com.aumofinance.app.reports.financials.financialposition

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.aumofinance.app.reports.financials.FinancialPositionReport
import com.aumofinance.app.reports.financials.FinancialReportScaffold
import com.aumofinance.app.reports.financials.ReportDivider
import com.aumofinance.app.reports.financials.ReportRow
import com.aumofinance.app.reports.financials.ReportSectionTitle
import com.aumofinance.app.ui.theme.AumoColors

/** Statement of Financial Position (Neraca): Aset = Liabilitas + Ekuitas. Padanan Compose dari activity_financial_position.xml. */
@Composable
fun FinancialPositionScreen(report: FinancialPositionReport?) {
    FinancialReportScaffold(periodName = report?.selectedPeriodName) {
        if (report == null) return@FinancialReportScaffold

        ReportSectionTitle("Aset")
        report.assetAccounts.forEach { ReportRow(it.accountName, it.amount, indent = true) }
        ReportRow("Total Aset", report.totalAssets, bold = true)

        ReportSectionTitle("Liabilitas")
        report.liabilityAccounts.forEach { ReportRow(it.accountName, it.amount, indent = true) }
        ReportRow("Total Liabilitas", report.totalLiabilities, bold = true)

        ReportSectionTitle("Ekuitas")
        // Baris "Retained Earnings" sudah termasuk di akhir list ini dari backend.
        report.equityAccounts.forEach { ReportRow(it.accountName, it.amount, indent = true) }
        ReportRow("Total Ekuitas", report.totalEquity, bold = true)

        ReportDivider()
        ReportRow("Total Liabilitas + Ekuitas", report.totalLiabilitiesAndEquity, bold = true)

        Text(
            text = if (report.isBalanced) "Neraca Balance" else "Neraca TIDAK Balance",
            color = if (report.isBalanced) AumoColors.Good else AumoColors.Bad,
            fontSize = MaterialTheme.typography.labelMedium.fontSize,
            modifier = Modifier.padding(top = 12.dp),
        )
    }
}
