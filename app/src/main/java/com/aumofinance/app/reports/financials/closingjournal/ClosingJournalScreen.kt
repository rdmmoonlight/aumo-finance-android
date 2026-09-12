package com.aumofinance.app.reports.financials.closingjournal

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.reports.financials.ClosingJournalReport
import com.aumofinance.app.reports.financials.FinancialReportScaffold
import com.aumofinance.app.reports.financials.ReportDivider
import com.aumofinance.app.reports.financials.ReportRow
import com.aumofinance.app.reports.financials.ReportSectionTitle
import com.aumofinance.app.ui.theme.AumoColors

/**
 * Read-only: entri Closing bersifat system-generated (dihitung on-the-fly
 * dari Trial Balance oleh backend, TIDAK PERNAH tersimpan sebagai entri
 * jurnal sungguhan) — tidak ada tombol tambah/edit/hapus di halaman ini.
 * Padanan Compose dari activity_closing_journal.xml.
 */
@Composable
fun ClosingJournalScreen(report: ClosingJournalReport?) {
    val data = report?.closingJournal

    FinancialReportScaffold(periodName = report?.selectedPeriodName) {
        Text(
            text =
                "Laba Bersih (ditutup ke ${data?.retainedEarningsAccountName ?: "Retained Earnings"}): " +
                    CurrencyFormatter.format(data?.netIncome ?: 0.0),
            color = AumoColors.TextPrimary,
            fontWeight = FontWeight.Bold,
            fontSize = MaterialTheme.typography.bodyLarge.fontSize,
            modifier = Modifier.padding(top = 4.dp),
        )

        data?.groups?.forEach { group ->
            ReportSectionTitle(group.description)
            group.lines.forEach { line ->
                val amount = if (line.debit > 0) line.debit else -line.credit
                ReportRow("${line.referenceNumber} - ${line.accountName}", amount, indent = true)
            }
            ReportRow("Total", group.totalDebit, bold = true)
            ReportDivider()
        }
    }
}
