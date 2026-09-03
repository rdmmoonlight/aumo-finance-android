package com.aumofinance.app.home

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.ui.icons.TablerIcon
import com.aumofinance.app.ui.icons.TablerIcons
import com.aumofinance.app.ui.theme.AumoColors
import java.util.Calendar as JavaCalendar

// Ikon di Home dulu sempat memakai library eksternal
// `br.com.devsrsouza.compose.icons:tabler` (ImageVector) — dihapus karena
// artifact ID/versi yang dipakai salah (seharusnya `tabler-icons:1.1.1`,
// bukan `tabler:0.2.0`) dan menambah jcenter() untuk "memperbaikinya" tidak
// pernah bisa berhasil karena JCenter sudah mati total sejak Feb 2022.
// Sekarang HomeScreen memakai TablerIcon (font glyph, lihat
// ui/icons/TablerIcons.kt) — sistem yang SAMA dengan yang dipakai di
// Journal Entry, supaya cuma ada SATU sumber Tabler Icons di seluruh app.

/** One menu item card on the Home screen. */
data class HomeMenuItem(
    val title: String,
    val subtitle: String,
    val icon: String,
    val onClick: () -> Unit
)

/**
 * Data required by the Home page: featured cards (Dashboard at the top, Reports
 * at the bottom) and 4 core menu items (Journal Entry, General Journal, Periods,
 * COA) displayed as a 2-column grid in between.
 *
 * Intentionally ONLY 6 cards per instruction — the 13 other report pages are not
 * displayed individually on the Home screen, but unified under a single "Reports"
 * card (see ReportsMenuActivity).
 */
@Composable
fun HomeScreen(
    dashboard: HomeMenuItem,
    journalEntry: HomeMenuItem,
    generalJournal: HomeMenuItem,
    periods: HomeMenuItem,
    coa: HomeMenuItem,
    reports: HomeMenuItem,
    onSettingsClick: () -> Unit
) {
    val greetingMessage = remember { getDynamicGreeting() }

    Scaffold(
        containerColor = AumoColors.Background,
        topBar = { HomeTopBar(onSettingsClick = onSettingsClick) }
    ) { innerPadding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 12.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp)
        ) {
            item {
                Text(
                    text = "Assalamu'alaikum wr. wb.",
                    color = AumoColors.TextMuted,
                    fontSize = MaterialTheme.typography.bodyMedium.fontSize
                )
                Spacer(modifier = Modifier.height(2.dp))
                Text(
                    text = greetingMessage,
                    color = AumoColors.TextPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.headlineSmall.fontSize
                )
            }

            // Featured card #1 — Dashboard: summary view placed at the top.
            item { FeaturedMenuCard(item = dashboard) }

            item {
                Text(
                    text = "MAIN MENU",
                    color = AumoColors.TextMuted,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.labelMedium.fontSize
                )
            }

            // 2-column grid for the 4 core items: inputs & master data.
            item {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Row(
                        horizontalArrangement = Arrangement.spacedBy(12.dp),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        GridMenuCard(item = journalEntry, modifier = Modifier.weight(1f))
                        GridMenuCard(item = generalJournal, modifier = Modifier.weight(1f))
                    }
                    Row(
                        horizontalArrangement = Arrangement.spacedBy(12.dp),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        GridMenuCard(item = periods, modifier = Modifier.weight(1f))
                        GridMenuCard(item = coa, modifier = Modifier.weight(1f))
                    }
                }
            }

            item {
                Text(
                    text = "REPORTS",
                    color = AumoColors.TextMuted,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.labelMedium.fontSize
                )
            }

            // Featured card #2 — Reports: entry point to all 13 report pages
            // (grouped inside ReportsMenuActivity).
            item { FeaturedMenuCard(item = reports) }

            item { Spacer(modifier = Modifier.height(8.dp)) }
        }
    }
}

@Composable
private fun HomeTopBar(onSettingsClick: () -> Unit) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .background(AumoColors.Background)
            .padding(horizontal = 16.dp, vertical = 14.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = "AumoFinance",
                color = AumoColors.TextPrimary,
                fontWeight = FontWeight.Bold,
                fontSize = MaterialTheme.typography.titleLarge.fontSize
            )
            Text(
                text = "Simple, neat, and accurate bookkeeping",
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.bodySmall.fontSize
            )
        }
        Box(
            modifier = Modifier
                .size(40.dp)
                .clip(CircleShape)
                .background(AumoColors.SurfaceElevated)
                .clickable(onClick = onSettingsClick),
            contentAlignment = Alignment.Center
        ) {
            TablerIcon(
                glyph = TablerIcons.Settings,
                tint = AumoColors.TextPrimary
            )
        }
    }
}

/** Large full-width card — used for Dashboard & Reports. */
@Composable
private fun FeaturedMenuCard(item: HomeMenuItem) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(18.dp))
            .background(AumoColors.Primary)
            .clickable(onClick = item.onClick)
            .padding(18.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Box(
            modifier = Modifier
                .size(52.dp)
                .clip(RoundedCornerShape(14.dp))
                .background(AumoColors.SurfaceElevated),
            contentAlignment = Alignment.Center
        ) {
            TablerIcon(
                glyph = item.icon,
                tint = AumoColors.TextPrimary,
                size = 26.dp
            )
        }
        Spacer(modifier = Modifier.width(14.dp))
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = item.title,
                color = AumoColors.TextPrimary,
                fontWeight = FontWeight.Bold,
                fontSize = MaterialTheme.typography.titleMedium.fontSize
            )
            Spacer(modifier = Modifier.height(2.dp))
            Text(
                text = item.subtitle,
                color = AumoColors.TextPrimary.copy(alpha = 0.75f),
                fontSize = MaterialTheme.typography.bodySmall.fontSize
            )
        }
        TablerIcon(
            glyph = TablerIcons.ChevronRight,
            tint = AumoColors.TextPrimary.copy(alpha = 0.75f)
        )
    }
}

/** Small card for the 2-column grid — used for the 4 core items. */
@Composable
private fun GridMenuCard(item: HomeMenuItem, modifier: Modifier = Modifier) {
    Column(
        modifier = modifier
            .clip(RoundedCornerShape(16.dp))
            .background(AumoColors.Surface)
            .clickable(onClick = item.onClick)
            .padding(16.dp)
    ) {
        Box(
            modifier = Modifier
                .size(44.dp)
                .clip(RoundedCornerShape(12.dp))
                .background(AumoColors.SurfaceElevated),
            contentAlignment = Alignment.Center
        ) {
            TablerIcon(
                glyph = item.icon,
                tint = AumoColors.Primary,
                size = 22.dp
            )
        }
        Spacer(modifier = Modifier.height(12.dp))
        Text(
            text = item.title,
            color = AumoColors.TextPrimary,
            fontWeight = FontWeight.SemiBold,
            fontSize = MaterialTheme.typography.bodyLarge.fontSize
        )
        Spacer(modifier = Modifier.height(2.dp))
        Text(
            text = item.subtitle,
            color = AumoColors.TextMuted,
            fontSize = MaterialTheme.typography.bodySmall.fontSize
        )
    }
}

/** Determines time-based greeting message. */
private fun getDynamicGreeting(): String {
    val hour = JavaCalendar.getInstance().get(JavaCalendar.HOUR_OF_DAY)
    return when (hour) {
        in 4..11 -> "Good morning"
        in 12..16 -> "Good afternoon"
        in 17..20 -> "Good evening"
        else -> "Good night"
    }
}

/** Default Tabler icons for each Home card — separated to keep HomeActivity concise. */
object HomeIcons {
    val Dashboard = TablerIcons.LayoutDashboard
    val JournalEntry = TablerIcons.FilePlus
    val GeneralJournal = TablerIcons.Book
    val Periods = TablerIcons.Calendar
    val Coa = TablerIcons.GitFork
    val Reports = TablerIcons.ReportAnalytics
}
