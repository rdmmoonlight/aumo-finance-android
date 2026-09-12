package com.aumofinance.app.dashboard

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.ui.theme.AumoColors

/** Padanan Compose dari activity_dashboard.xml. */
@Composable
fun DashboardScreen(summary: DashboardSummary?) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(
            modifier =
                Modifier
                    .fillMaxSize()
                    .padding(innerPadding)
                    .verticalScroll(rememberScrollState())
                    .padding(20.dp),
        ) {
            if (summary == null || !summary.hasPeriodSelected) {
                Text(
                    text = "Belum ada periode dipilih",
                    color = AumoColors.TextMuted,
                    fontSize = MaterialTheme.typography.labelMedium.fontSize,
                )
                return@Column
            }

            Text(
                text = summary.selectedPeriodName ?: "-",
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.labelMedium.fontSize,
            )
            if (summary.isPeriodClosed) {
                Text(
                    text = "Periode Ditutup",
                    color = AumoColors.Bad,
                    fontSize = MaterialTheme.typography.labelSmall.fontSize,
                    modifier = Modifier.padding(top = 4.dp),
                )
            }

            // Ringkasan Neraca
            Column(
                modifier =
                    Modifier
                        .fillMaxWidth()
                        .padding(top = 16.dp)
                        .background(AumoColors.Surface)
                        .padding(16.dp),
            ) {
                BalanceLine(label = "Total Aset", value = summary.totalAssets, valueSize = MaterialTheme.typography.titleMedium.fontSize, bold = true)
                BalanceLine(label = "Total Liabilitas", value = summary.totalLiabilities, valueSize = MaterialTheme.typography.bodyLarge.fontSize, topPadding = 12.dp)
                BalanceLine(label = "Total Ekuitas", value = summary.totalEquity, valueSize = MaterialTheme.typography.bodyLarge.fontSize, topPadding = 12.dp)
            }

            // Ringkasan Laba Rugi
            Column(
                modifier =
                    Modifier
                        .fillMaxWidth()
                        .padding(top = 12.dp)
                        .background(AumoColors.SurfaceElevated)
                        .padding(16.dp),
            ) {
                Text("Laba Bersih Periode Ini", color = AumoColors.TextMuted, fontSize = MaterialTheme.typography.labelSmall.fontSize)
                Text(
                    text = CurrencyFormatter.format(summary.netIncome),
                    color = AumoColors.Good,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.headlineSmall.fontSize,
                )
                Text(
                    text = "Pendapatan ${CurrencyFormatter.format(summary.totalRevenue)}  \u2022  Beban ${CurrencyFormatter.format(summary.totalExpenses)}",
                    color = AumoColors.TextMuted,
                    fontSize = MaterialTheme.typography.labelSmall.fontSize,
                    modifier = Modifier.padding(top = 6.dp),
                )
            }

            CashBankSection(title = "Kas", entries = summary.cashAccounts, total = summary.totalCashOnHand, totalLabel = "Total Kas")
            CashBankSection(title = "Bank", entries = summary.bankAccounts, total = summary.totalBankBalance, totalLabel = "Total Bank")
        }
    }
}

@Composable
private fun BalanceLine(
    label: String,
    value: Double,
    valueSize: androidx.compose.ui.unit.TextUnit,
    bold: Boolean = false,
    topPadding: androidx.compose.ui.unit.Dp = 0.dp,
) {
    Column(modifier = Modifier.padding(top = topPadding)) {
        Text(label, color = AumoColors.TextMuted, fontSize = MaterialTheme.typography.labelSmall.fontSize)
        Text(
            text = CurrencyFormatter.format(value),
            color = AumoColors.TextPrimary,
            fontWeight = if (bold) FontWeight.Bold else FontWeight.Normal,
            fontSize = valueSize,
        )
    }
}

@Composable
private fun CashBankSection(
    title: String,
    entries: List<CashAccountEntry>,
    total: Double,
    totalLabel: String,
) {
    Text(
        text = title,
        color = AumoColors.TextPrimary,
        fontWeight = FontWeight.Bold,
        fontSize = MaterialTheme.typography.bodyMedium.fontSize,
        modifier = Modifier.padding(top = 20.dp),
    )
    Column(
        modifier = Modifier.fillMaxWidth().padding(top = 8.dp),
        verticalArrangement = Arrangement.spacedBy(0.dp),
    ) {
        entries.forEach { entry ->
            Row(
                modifier = Modifier.fillMaxWidth().padding(vertical = 6.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
            ) {
                Text(entry.accountName, color = AumoColors.TextPrimary, fontSize = MaterialTheme.typography.bodySmall.fontSize, modifier = Modifier.weight(1f))
                Text(CurrencyFormatter.format(entry.balance), color = AumoColors.TextPrimary, fontSize = MaterialTheme.typography.bodySmall.fontSize)
            }
        }
    }
    Text(
        text = "$totalLabel: ${CurrencyFormatter.format(total)}",
        color = AumoColors.TextMuted,
        fontSize = MaterialTheme.typography.labelSmall.fontSize,
        modifier = Modifier.padding(top = 6.dp),
    )
}
