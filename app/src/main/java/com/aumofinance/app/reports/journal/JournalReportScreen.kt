package com.aumofinance.app.reports.journal

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.ui.icons.TablerIcon
import com.aumofinance.app.ui.icons.TablerIcons
import com.aumofinance.app.ui.theme.AumoColors
import java.text.SimpleDateFormat
import java.util.Locale

// Lebar indentasi/tab nomor referensi baris kredit — dipakai juga untuk
// menggeser mundur (backspace) nominal debit sejauh jarak yang sama, supaya
// nominal debit & kredit sengaja tidak sejajar. Sama persis dengan versi
// View/XML sebelumnya (RecyclerView + JournalReportAdapter, sekarang dihapus).
private const val CREDIT_INDENT = "        "

private val inputDateTimeFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US)
private val displayDateFormat = SimpleDateFormat("dd MMMM yyyy", Locale("in", "ID"))
private val displayTimestampFormat = SimpleDateFormat("dd/MM HH:mm", Locale("in", "ID"))

/**
 * Halaman General/Adjusting Journal, ditulis dengan Jetpack Compose
 * (sebelumnya RecyclerView + View/XML biasa — activity_general_journal_report.xml,
 * item_journal_date_header.xml, item_journal_entry_group.xml,
 * item_journal_report_line.xml, JournalReportAdapter, semuanya dihapus).
 * Dipakai bersama oleh GeneralJournalReportActivity (showToggle=true,
 * showActions dikontrol toggle Edit/Selesai) dan AdjustingJournalReportActivity
 * (showToggle=false, showActions selalu true). Ikon pensil/trash memakai
 * TablerIcon — sama seperti Journal Entry, Home & Periods.
 */
@Composable
fun JournalReportScreen(
    entries: List<JournalReportEntry>,
    selectedPeriodName: String?,
    defaultPeriodLabel: String,
    showToggle: Boolean,
    showActions: Boolean,
    onToggleShowActions: (Boolean) -> Unit,
    onEdit: (JournalReportEntry) -> Unit,
    onDeleteRequest: (JournalReportEntry) -> Unit,
) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(
            modifier =
                Modifier
                    .fillMaxSize()
                    .padding(innerPadding)
                    .padding(16.dp),
        ) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text(
                    text = "${selectedPeriodName ?: defaultPeriodLabel} \u00B7 Nominal dalam Rupiah",
                    color = AumoColors.TextMuted,
                    fontSize = MaterialTheme.typography.bodyMedium.fontSize,
                    modifier = Modifier.weight(1f),
                )
                if (showToggle) {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        modifier =
                            Modifier
                                .background(
                                    if (showActions) AumoColors.Primary else AumoColors.SurfaceElevated,
                                    RoundedCornerShape(8.dp),
                                )
                                .clickable { onToggleShowActions(!showActions) }
                                .padding(12.dp, 6.dp),
                    ) {
                        Text(
                            text = if (showActions) "Selesai" else "Edit",
                            color = Color.White,
                            fontWeight = FontWeight.Bold,
                            fontSize = MaterialTheme.typography.labelSmall.fontSize,
                        )
                    }
                }
            }

            val grouped =
                remember(entries) {
                    entries.groupBy { it.entryDate.substringBefore("T") }.toSortedMap()
                }

            LazyColumn(
                modifier = Modifier.weight(1f).fillMaxWidth(),
                contentPadding = PaddingValues(top = 8.dp, bottom = 16.dp),
            ) {
                grouped.forEach { (dateKey, entriesForDate) ->
                    item(key = "header-$dateKey") {
                        Text(
                            text = formatDateLabel(dateKey),
                            color = AumoColors.TextMuted,
                            fontWeight = FontWeight.Bold,
                            fontSize = MaterialTheme.typography.titleMedium.fontSize,
                            modifier = Modifier.padding(top = 12.dp, bottom = 4.dp),
                        )
                    }
                    items(entriesForDate, key = { it.id }) { entry ->
                        EntryCard(
                            entry = entry,
                            showActions = showActions,
                            onEdit = { onEdit(entry) },
                            onDelete = { onDeleteRequest(entry) },
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun EntryCard(
    entry: JournalReportEntry,
    showActions: Boolean,
    onEdit: () -> Unit,
    onDelete: () -> Unit,
) {
    Column(
        modifier =
            Modifier
                .fillMaxWidth()
                .padding(bottom = 8.dp)
                .background(AumoColors.Surface, RoundedCornerShape(8.dp))
                .padding(12.dp),
    ) {
        Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.fillMaxWidth()) {
            Text(
                text = entry.transactionNumber,
                color = AumoColors.TextPrimary,
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
                modifier = Modifier.weight(1f),
            )
            Text(
                text = formatTimestampLabel(entry),
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.labelSmall.fontSize,
                modifier = Modifier.padding(end = 6.dp),
            )
            if (showActions) {
                // Ikon pensil/trash sengaja pakai clickable polos (bukan
                // IconButton) supaya benar-benar compact — IconButton
                // Material3 memaksa target sentuh minimum ~48dp yang bikin
                // ikon terlihat besar lagi, sama seperti masalah ImageButton
                // di versi View/XML sebelumnya.
                TablerIcon(
                    TablerIcons.Edit,
                    tint = AumoColors.TextSecondary,
                    size = 16.dp,
                    modifier = Modifier.clickable(onClick = onEdit),
                )
                Spacer(modifier = Modifier.width(10.dp))
                TablerIcon(
                    TablerIcons.Trash,
                    tint = AumoColors.Bad,
                    size = 16.dp,
                    modifier = Modifier.clickable(onClick = onDelete),
                )
            }
        }

        Spacer(modifier = Modifier.height(6.dp))

        entry.lines.sortedBy { it.lineOrder }.forEach { line ->
            JournalLineRow(line)
        }
    }
}

// Nama akun + deskripsi (pemisah " - "); nominal tanpa "Rp" (mata uang
// sudah dinyatakan sekali di header halaman). Debit & kredit sama-sama
// rata kanan. Kredit mepet penuh ke tepi; nominal debit sengaja digeser
// mundur (backspace) sejauh lebar CREDIT_INDENT — dicapai dengan spasi
// kosong di akhir teks nominal debit, sama seperti versi View/XML.
@Composable
private fun JournalLineRow(line: JournalReportLine) {
    val label =
        buildString {
            append(line.referenceNumber).append(" - ").append(line.accountName)
            if (!line.lineDescription.isNullOrBlank()) {
                append(" - ").append(line.lineDescription)
            }
        }
    Row(modifier = Modifier.fillMaxWidth().padding(vertical = 1.dp)) {
        if (line.debit > 0) {
            Text(
                text = label,
                color = AumoColors.TextPrimary,
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
                modifier = Modifier.weight(1f),
            )
            Text(
                text = CurrencyFormatter.formatBare(line.debit) + CREDIT_INDENT,
                color = AumoColors.TextPrimary,
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
            )
        } else {
            // Kredit: indentasi satu tab (spasi) dari debit, nominal di
            // kolom kanan mepet penuh ke tepi.
            Text(
                text = "$CREDIT_INDENT$label",
                color = AumoColors.TextPrimary,
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
                modifier = Modifier.weight(1f),
            )
            Text(
                text = CurrencyFormatter.formatBare(line.credit),
                color = AumoColors.TextPrimary,
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
            )
        }
    }
}

private fun formatDateLabel(dateKey: String): String =
    try {
        displayDateFormat.format(inputDateTimeFormat.parse("${dateKey}T00:00:00")!!)
    } catch (e: Exception) {
        dateKey
    }

// Timestamp created/edited, ditaruh compact di sebelah kanan baris nomor
// transaksi (tepat di bawah tanggal entri). Hanya tampilkan "Diubah" jika
// entry pernah diedit (updatedAt beda dari createdAt).
private fun formatTimestampLabel(entry: JournalReportEntry): String {
    val createdLabel = formatTimestamp(entry.createdAt)
    val updated = entry.updatedAt
    return if (updated != null && updated != entry.createdAt) {
        "Dibuat $createdLabel \u00B7 Diubah ${formatTimestamp(updated)}"
    } else {
        "Dibuat $createdLabel"
    }
}

private fun formatTimestamp(raw: String): String =
    try {
        val normalized = raw.substringBefore(".").let { if (it.length == 10) "${it}T00:00:00" else it }
        displayTimestampFormat.format(inputDateTimeFormat.parse(normalized)!!)
    } catch (e: Exception) {
        raw
    }
