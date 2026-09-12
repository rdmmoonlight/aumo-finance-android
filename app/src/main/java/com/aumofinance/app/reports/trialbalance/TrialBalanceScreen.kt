package com.aumofinance.app.reports.trialbalance

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.ui.theme.AumoColors

// Padanan Compose dari activity_trial_balance.xml + TrialBalanceAdapter/item_trial_balance_row.
// Dipakai bareng oleh 3 varian: Unadjusted, Adjusted, Post-Closing (beda cuma
// parameter "type" saat load() di masing-masing ViewModel).
@Composable
fun TrialBalanceScreen(
    report: TrialBalanceReport?,
    fallbackTitle: String,
) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(modifier = Modifier.fillMaxSize().padding(innerPadding).padding(16.dp)) {
            Text(
                text = report?.reportTitle ?: fallbackTitle,
                color = AumoColors.TextPrimary,
                fontWeight = FontWeight.Bold,
                fontSize = MaterialTheme.typography.titleMedium.fontSize,
            )
            Text(
                text = report?.selectedPeriodName ?: "Belum ada periode dipilih",
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.labelMedium.fontSize,
                modifier = Modifier.padding(top = 2.dp),
            )

            LazyColumn(modifier = Modifier.weight(1f).padding(top = 8.dp)) {
                items(report?.rows ?: emptyList()) { row -> TrialBalanceRowItem(row) }
            }

            if (report != null) {
                Text(
                    text = "Debit: ${CurrencyFormatter.format(report.totalDebit)}   Kredit: ${CurrencyFormatter.format(report.totalCredit)}",
                    color = AumoColors.TextPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.bodyMedium.fontSize,
                    modifier = Modifier.padding(top = 8.dp),
                )
                Text(
                    text = if (report.isBalanced) "Balanced" else "Unbalanced",
                    color = if (report.isBalanced) AumoColors.Good else AumoColors.Bad,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.labelMedium.fontSize,
                    modifier = Modifier.padding(top = 4.dp),
                )
            }
        }
    }
}

@Composable
private fun TrialBalanceRowItem(row: TrialBalanceRow) {
    Row(modifier = Modifier.fillMaxWidth().padding(vertical = 6.dp)) {
        Text(
            text = "${row.referenceNumber} - ${row.accountName}",
            color = AumoColors.TextPrimary,
            fontSize = MaterialTheme.typography.bodySmall.fontSize,
            modifier = Modifier.weight(1f),
        )
        Text(
            text = if (row.debit > 0) CurrencyFormatter.format(row.debit) else "",
            color = AumoColors.TextPrimary,
            fontSize = MaterialTheme.typography.bodySmall.fontSize,
            modifier = Modifier.padding(end = 12.dp),
        )
        Text(
            text = if (row.credit > 0) CurrencyFormatter.format(row.credit) else "",
            color = AumoColors.TextPrimary,
            fontSize = MaterialTheme.typography.bodySmall.fontSize,
        )
    }
}
