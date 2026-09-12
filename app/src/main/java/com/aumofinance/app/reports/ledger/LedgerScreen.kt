package com.aumofinance.app.reports.ledger

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.ui.theme.AumoColors
import java.text.SimpleDateFormat
import java.util.Locale

// Padanan Compose dari activity_general_ledger.xml + LedgerAdapter/item_ledger_account/item_ledger_line.
// Dipakai bareng oleh halaman Permanent & Temporary (satu-satunya beda: parameter isTemporary saat load()).
@Composable
fun LedgerScreen(report: LedgerResponse?) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(modifier = Modifier.fillMaxSize().padding(innerPadding).padding(16.dp)) {
            Text(
                text = report?.selectedPeriodName ?: "Belum ada periode dipilih",
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.labelMedium.fontSize,
            )
            LazyColumn(modifier = Modifier.fillMaxSize().padding(top = 8.dp)) {
                items(report?.ledgers ?: emptyList()) { account -> LedgerAccountCard(account) }
            }
        }
    }
}

@Composable
private fun LedgerAccountCard(account: LedgerAccount) {
    Column(
        modifier =
            Modifier
                .fillMaxWidth()
                .padding(bottom = 12.dp)
                .background(AumoColors.Surface, RoundedCornerShape(8.dp))
                .padding(14.dp),
    ) {
        Text(
            text = "${account.referenceNumber} - ${account.accountName}",
            color = AumoColors.TextPrimary,
            fontWeight = FontWeight.Bold,
            fontSize = MaterialTheme.typography.bodyMedium.fontSize,
        )
        account.lines.forEach { line -> LedgerLineRow(line) }
        Text(
            text = "Saldo Akhir: ${CurrencyFormatter.format(account.endingBalance)}",
            color = AumoColors.TextPrimary,
            fontWeight = FontWeight.Bold,
            fontSize = MaterialTheme.typography.bodySmall.fontSize,
            modifier = Modifier.padding(top = 6.dp),
        )
    }
}

private val inputDateFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US)
private val displayDateFormat = SimpleDateFormat("dd/MM", Locale("in", "ID"))

private fun formatDate(iso: String): String =
    try {
        displayDateFormat.format(inputDateFormat.parse(iso)!!)
    } catch (e: Exception) {
        iso
    }

@Composable
private fun LedgerLineRow(line: LedgerLine) {
    Row(modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp)) {
        Text(
            text = formatDate(line.entryDate),
            color = AumoColors.TextMuted,
            fontSize = MaterialTheme.typography.labelSmall.fontSize,
            modifier = Modifier.padding(end = 8.dp),
        )
        Text(
            text = line.description ?: "",
            color = AumoColors.TextPrimary,
            fontSize = MaterialTheme.typography.labelSmall.fontSize,
            modifier = Modifier.weight(1f),
        )
        val amount = if (line.debit > 0) line.debit else -line.credit
        Text(
            text = CurrencyFormatter.format(amount),
            color = AumoColors.TextPrimary,
            fontSize = MaterialTheme.typography.labelSmall.fontSize,
            modifier = Modifier.padding(end = 8.dp),
        )
        Text(
            text = CurrencyFormatter.format(line.runningBalance),
            color = AumoColors.TextMuted,
            fontSize = MaterialTheme.typography.labelSmall.fontSize,
        )
    }
}
