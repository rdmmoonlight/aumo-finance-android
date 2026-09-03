package com.aumofinance.app.reports.menu

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.outlined.AccountBalance
import androidx.compose.material.icons.outlined.Balance
import androidx.compose.material.icons.outlined.ChevronRight
import androidx.compose.material.icons.outlined.MenuBook
import androidx.compose.material.icons.outlined.Payments
import androidx.compose.material.icons.outlined.PieChart
import androidx.compose.material.icons.outlined.ReceiptLong
import androidx.compose.material.icons.outlined.Savings
import androidx.compose.material.icons.outlined.TableChart
import androidx.compose.material.icons.outlined.TrendingUp
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.ui.theme.AumoColors

data class ReportMenuItem(
    val title: String,
    val icon: ImageVector,
    val onClick: () -> Unit
)

data class ReportMenuSection(
    val title: String,
    val items: List<ReportMenuItem>
)

@Composable
fun ReportsMenuScreen(
    sections: List<ReportMenuSection>,
    onBackClick: () -> Unit
) {
    Scaffold(
        containerColor = AumoColors.Background,
        topBar = {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .background(AumoColors.Background)
                    .padding(horizontal = 8.dp, vertical = 10.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                IconButton(onClick = onBackClick) {
                    Icon(
                        imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                        contentDescription = "Kembali",
                        tint = AumoColors.TextPrimary
                    )
                }
                Text(
                    text = "Reports",
                    color = AumoColors.TextPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.titleLarge.fontSize
                )
            }
        }
    ) { innerPadding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp),
            verticalArrangement = Arrangement.spacedBy(20.dp)
        ) {
            items(sections) { section ->
                Column {
                    Text(
                        text = section.title.uppercase(),
                        color = AumoColors.TextMuted,
                        fontWeight = FontWeight.Bold,
                        fontSize = MaterialTheme.typography.labelMedium.fontSize
                    )
                    androidx.compose.foundation.layout.Spacer(modifier = Modifier.padding(top = 8.dp))
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .clip(RoundedCornerShape(16.dp))
                            .background(AumoColors.Surface)
                    ) {
                        section.items.forEachIndexed { index, item ->
                            ReportRow(item = item)
                            if (index != section.items.lastIndex) {
                                androidx.compose.material3.Divider(
                                    color = AumoColors.Border.copy(alpha = 0.4f),
                                    thickness = 1.dp,
                                    modifier = Modifier.padding(start = 56.dp)
                                )
                            }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun ReportRow(item: ReportMenuItem) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clickable(onClick = item.onClick)
            .padding(horizontal = 14.dp, vertical = 14.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Icon(
            imageVector = item.icon,
            contentDescription = null,
            tint = AumoColors.Primary,
            modifier = Modifier.padding(end = 14.dp)
        )
        Text(
            text = item.title,
            color = AumoColors.TextPrimary,
            fontSize = MaterialTheme.typography.bodyLarge.fontSize,
            modifier = Modifier.weight(1f)
        )
        Icon(
            imageVector = Icons.Outlined.ChevronRight,
            contentDescription = null,
            tint = AumoColors.TextMuted
        )
    }
}

/** Ikon bawaan tiap baris laporan, dipisah agar Activity ringkas. */
object ReportMenuIcons {
    val AdjustingJournal = Icons.Outlined.MenuBook
    val LedgerPermanent = Icons.Outlined.AccountBalance
    val LedgerTemporary = Icons.Outlined.AccountBalance
    val TrialBalance = Icons.Outlined.Balance
    val Worksheet = Icons.Outlined.TableChart
    val IncomeStatement = Icons.Outlined.TrendingUp
    val RetainedEarnings = Icons.Outlined.Savings
    val FinancialPosition = Icons.Outlined.PieChart
    val CashFlow = Icons.Outlined.Payments
    val ClosingJournal = Icons.Outlined.ReceiptLong
}
