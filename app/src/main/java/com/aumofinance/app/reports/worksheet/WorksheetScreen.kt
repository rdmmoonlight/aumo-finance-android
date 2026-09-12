package com.aumofinance.app.reports.worksheet

import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.ui.theme.AumoColors

private val columnHeaders =
    listOf(
        "Akun", "TB Debit", "TB Kredit", "Adj Debit", "Adj Kredit",
        "Adj.TB Debit", "Adj.TB Kredit", "L/R Debit", "L/R Kredit", "Neraca Debit", "Neraca Kredit",
    )

private fun fmt(value: Double): String = if (value == 0.0) "" else CurrencyFormatter.format(value)

/**
 * Footer WAJIB menampilkan 3 baris total standar akuntansi: 1) Total (sebelum
 * plug), 2) Laba/Rugi Bersih (plug ke Neraca), 3) Total Akhir (setelah plug).
 * Istilah "plug" tidak ditampilkan ke pengguna. Tabel 11 kolom terlalu lebar
 * untuk layar ponsel, jadi dibungkus horizontalScroll. Padanan Compose dari
 * activity_worksheet.xml (dulu dibangun programatik dengan LinearLayout).
 */
@Composable
fun WorksheetScreen(report: WorksheetReport?) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(modifier = Modifier.fillMaxSize().padding(innerPadding).padding(16.dp)) {
            Text(
                text = report?.selectedPeriodName ?: "Belum ada periode dipilih",
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.labelMedium.fontSize,
            )

            Column(
                modifier =
                    Modifier
                        .padding(top = 8.dp)
                        .horizontalScroll(rememberScrollState())
                        .verticalScroll(rememberScrollState()),
            ) {
                WorksheetRowView(columnHeaders, isHeader = true)

                report?.rows?.forEach { row ->
                    WorksheetRowView(
                        listOf(
                            row.accountName,
                            fmt(row.tbDebit), fmt(row.tbCredit),
                            fmt(row.adjDebit), fmt(row.adjCredit),
                            fmt(row.adjTbDebit), fmt(row.adjTbCredit),
                            fmt(row.isDebit), fmt(row.isCredit),
                            fmt(row.bsDebit), fmt(row.bsCredit),
                        ),
                        isHeader = false,
                    )
                }

                report?.totals?.let { totals ->
                    WorksheetRowView(
                        listOf(
                            "Total", fmt(totals.tbDebit), fmt(totals.tbCredit), fmt(totals.adjDebit), fmt(totals.adjCredit),
                            fmt(totals.adjTbDebit), fmt(totals.adjTbCredit), fmt(totals.isDebit), fmt(totals.isCredit),
                            fmt(totals.bsDebit), fmt(totals.bsCredit),
                        ),
                        isHeader = true,
                    )

                    val netIncome = totals.netIncome
                    val isPositive = netIncome >= 0
                    // Plug: Laba Bersih menambah sisi Debit L/R (menyeimbangkan L/R yang
                    // lebih besar di Kredit) dan sisi Kredit Neraca (menyeimbangkan Neraca
                    // yang lebih besar di Debit).
                    WorksheetRowView(
                        listOf(
                            if (isPositive) "Laba Bersih" else "Rugi Bersih",
                            "", "", "", "", "", "",
                            if (isPositive) fmt(netIncome) else "", if (!isPositive) fmt(-netIncome) else "",
                            if (!isPositive) fmt(-netIncome) else "", if (isPositive) fmt(netIncome) else "",
                        ),
                        isHeader = false,
                    )

                    WorksheetRowView(
                        listOf(
                            "Total Akhir",
                            "", "", "", "",
                            fmt(totals.adjTbDebit), fmt(totals.adjTbCredit),
                            fmt(totals.isDebit + (if (isPositive) netIncome else 0.0)),
                            fmt(totals.isCredit + (if (!isPositive) -netIncome else 0.0)),
                            fmt(totals.bsDebit + (if (!isPositive) -netIncome else 0.0)),
                            fmt(totals.bsCredit + (if (isPositive) netIncome else 0.0)),
                        ),
                        isHeader = true,
                    )
                }
            }
        }
    }
}

@Composable
private fun WorksheetRowView(
    cells: List<String>,
    isHeader: Boolean,
) {
    Row {
        cells.forEachIndexed { index, text ->
            Text(
                text = text,
                color = if (isHeader) AumoColors.Primary else AumoColors.TextPrimary,
                fontWeight = if (isHeader) FontWeight.Bold else FontWeight.Normal,
                fontSize = 12.sp,
                modifier =
                    Modifier
                        .width(if (index == 0) 140.dp else 110.dp)
                        .padding(horizontal = 12.dp, vertical = 8.dp),
            )
        }
    }
}
