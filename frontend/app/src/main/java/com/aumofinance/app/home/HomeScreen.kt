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
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.outlined.AccountTree
import androidx.compose.material.icons.outlined.Assessment
import androidx.compose.material.icons.outlined.CalendarMonth
import androidx.compose.material.icons.outlined.ChevronRight
import androidx.compose.material.icons.outlined.MenuBook
import androidx.compose.material.icons.outlined.NoteAdd
import androidx.compose.material.icons.outlined.SpaceDashboard
import androidx.compose.material.icons.outlined.Settings
import androidx.compose.material3.Icon
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

/** Satu kotak menu di Home. */
data class HomeMenuItem(
    val title: String,
    val subtitle: String,
    val icon: ImageVector,
    val onClick: () -> Unit
)

/**
 * Data yang dibutuhkan Home page: kartu unggulan (Dashboard di atas, Reports
 * di bawah) dan 4 kotak menu inti (Journal Entry, General Journal, Periode,
 * COA) yang ditampilkan sebagai grid 2 kolom di antara keduanya.
 *
 * Sengaja HANYA 6 kotak sesuai instruksi — 13 halaman laporan lain tidak
 * ditampilkan satu-satu di Home, melainkan disatukan di balik satu kotak
 * "Reports" (lihat ReportsMenuActivity).
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
                    text = "Selamat datang kembali",
                    color = AumoColors.TextMuted,
                    fontSize = MaterialTheme.typography.bodyMedium.fontSize
                )
                Spacer(modifier = Modifier.height(2.dp))
                Text(
                    text = "Mau mulai dari mana?",
                    color = AumoColors.TextPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.headlineSmall.fontSize
                )
            }

            // Kartu unggulan #1 — Dashboard: ringkasan, jadi yang paling atas.
            item { FeaturedMenuCard(item = dashboard) }

            item {
                Text(
                    text = "MENU UTAMA",
                    color = AumoColors.TextMuted,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.labelMedium.fontSize
                )
            }

            // Grid 2 kolom untuk 4 kotak inti: input & data master.
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
                    text = "LAPORAN",
                    color = AumoColors.TextMuted,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.labelMedium.fontSize
                )
            }

            // Kartu unggulan #2 — Reports: pintu masuk ke seluruh 13 halaman
            // laporan (dikelompokkan di dalam ReportsMenuActivity).
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
                text = "Pembukuan sederhana, rapi, dan akurat",
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
            Icon(
                imageVector = Icons.Outlined.Settings,
                contentDescription = "Pengaturan",
                tint = AumoColors.TextPrimary
            )
        }
    }
}

/** Kartu besar full-width — dipakai untuk Dashboard & Reports. */
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
            Icon(
                imageVector = item.icon,
                contentDescription = null,
                tint = AumoColors.TextPrimary,
                modifier = Modifier.size(26.dp)
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
        Icon(
            imageVector = Icons.Outlined.ChevronRight,
            contentDescription = null,
            tint = AumoColors.TextPrimary.copy(alpha = 0.75f)
        )
    }
}

/** Kartu kecil untuk grid 2 kolom — dipakai untuk 4 kotak inti. */
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
            Icon(
                imageVector = item.icon,
                contentDescription = null,
                tint = AumoColors.Primary,
                modifier = Modifier.size(22.dp)
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

/** Ikon bawaan untuk tiap kotak Home — dipisah agar HomeActivity ringkas. */
object HomeIcons {
    val Dashboard = Icons.Outlined.SpaceDashboard
    val JournalEntry = Icons.Outlined.NoteAdd
    val GeneralJournal = Icons.Outlined.MenuBook
    val Periods = Icons.Outlined.CalendarMonth
    val Coa = Icons.Outlined.AccountTree
    val Reports = Icons.Outlined.Assessment
}
