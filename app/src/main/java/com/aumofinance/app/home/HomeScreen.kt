package com.aumofinance.app.home

import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
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
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.ui.icons.TablerIcon
import com.aumofinance.app.ui.icons.TablerIcons
import com.aumofinance.app.ui.theme.AumoColors
import java.util.Calendar as JavaCalendar

data class HomeMenuItem(
    val title: String,
    val subtitle: String,
    val icon: String,
    val onClick: () -> Unit,
)

@Composable
fun HomeScreen(
    dashboard: HomeMenuItem,
    journalEntry: HomeMenuItem,
    generalJournal: HomeMenuItem,
    periods: HomeMenuItem,
    coa: HomeMenuItem,
    reports: HomeMenuItem,
    isDbConnected: Boolean,
    onSettingsClick: () -> Unit,
) {
    val greetingMessage = remember { getDynamicGreeting() }

    Scaffold(
        containerColor = AumoColors.Background,
        topBar = {
            HomeTopBar(
                isDbConnected = isDbConnected,
                onSettingsClick = onSettingsClick,
            )
        },
    ) { innerPadding ->
        LazyColumn(
            modifier =
                Modifier
                    .fillMaxSize()
                    .padding(innerPadding),
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 12.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp),
        ) {
            item {
                Text(
                    text = "Assalaamu'alaikum wa rahmatullahi wa barakaatuh",
                    color = AumoColors.TextMuted,
                    fontSize = MaterialTheme.typography.bodyMedium.fontSize,
                )
                Spacer(modifier = Modifier.height(2.dp))
                Text(
                    text = greetingMessage,
                    color = AumoColors.TextPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.headlineSmall.fontSize,
                )
            }

            item { FeaturedMenuCard(item = dashboard) }

            item {
                Text(
                    text = "MAIN MENU",
                    color = AumoColors.TextMuted,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.labelMedium.fontSize,
                )
            }

            item {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Row(
                        horizontalArrangement = Arrangement.spacedBy(12.dp),
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        GridMenuCard(item = journalEntry, modifier = Modifier.weight(1f))
                        GridMenuCard(item = generalJournal, modifier = Modifier.weight(1f))
                    }
                    Row(
                        horizontalArrangement = Arrangement.spacedBy(12.dp),
                        modifier = Modifier.fillMaxWidth(),
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
                    fontSize = MaterialTheme.typography.labelMedium.fontSize,
                )
            }

            item { FeaturedMenuCard(item = reports) }

            item { Spacer(modifier = Modifier.height(8.dp)) }
        }
    }
}

@Composable
private fun HomeTopBar(
    isDbConnected: Boolean,
    onSettingsClick: () -> Unit,
) {
    val infiniteTransition = rememberInfiniteTransition(label = "db_indicator_blink")
    val alpha by infiniteTransition.animateFloat(
        initialValue = 0.3f,
        targetValue = 1.0f,
        animationSpec =
            infiniteRepeatable(
                animation = tween(durationMillis = 600),
                repeatMode = RepeatMode.Reverse,
            ),
        label = "alpha_anim",
    )

    val indicatorColor = if (isDbConnected) AumoColors.Good else Color(0xFFFFC107)
    val currentAlpha = if (isDbConnected) 1.0f else alpha

    Row(
        modifier =
            Modifier
                .fillMaxWidth()
                .background(AumoColors.Background)
                .padding(horizontal = 16.dp, vertical = 14.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Column(modifier = Modifier.weight(1f)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Text(
                    text = "Aumo Finance",
                    color = AumoColors.TextPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.titleLarge.fontSize,
                )
                Spacer(modifier = Modifier.width(8.dp))

                Box(
                    modifier =
                        Modifier
                            .size(10.dp)
                            .alpha(currentAlpha)
                            .clip(CircleShape)
                            .background(indicatorColor),
                )
            }
            Text(
                text = "Simple, neat, and accurate bookkeeping",
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
            )
        }
        Box(
            modifier =
                Modifier
                    .size(40.dp)
                    .clip(CircleShape)
                    .background(AumoColors.SurfaceElevated)
                    .clickable(onClick = onSettingsClick),
            contentAlignment = Alignment.Center,
        ) {
            TablerIcon(
                glyph = TablerIcons.Settings,
                tint = AumoColors.TextPrimary,
            )
        }
    }
}

@Composable
private fun FeaturedMenuCard(item: HomeMenuItem) {
    Row(
        modifier =
            Modifier
                .fillMaxWidth()
                .clip(RoundedCornerShape(18.dp))
                .background(AumoColors.Primary)
                .clickable(onClick = item.onClick)
                .padding(18.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Box(
            modifier =
                Modifier
                    .size(52.dp)
                    .clip(RoundedCornerShape(14.dp))
                    .background(AumoColors.SurfaceElevated),
            contentAlignment = Alignment.Center,
        ) {
            TablerIcon(
                glyph = item.icon,
                tint = AumoColors.TextPrimary,
                size = 26.dp,
            )
        }
        Spacer(modifier = Modifier.width(14.dp))
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = item.title,
                color = AumoColors.TextPrimary,
                fontWeight = FontWeight.Bold,
                fontSize = MaterialTheme.typography.titleMedium.fontSize,
            )
            Spacer(modifier = Modifier.height(2.dp))
            Text(
                text = item.subtitle,
                color = AumoColors.TextPrimary.copy(alpha = 0.75f),
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
            )
        }
        TablerIcon(
            glyph = TablerIcons.ChevronRight,
            tint = AumoColors.TextPrimary.copy(alpha = 0.75f),
        )
    }
}

@Composable
private fun GridMenuCard(
    item: HomeMenuItem,
    modifier: Modifier = Modifier,
) {
    Column(
        modifier =
            modifier
                .clip(RoundedCornerShape(16.dp))
                .background(AumoColors.Surface)
                .clickable(onClick = item.onClick)
                .padding(16.dp),
    ) {
        Box(
            modifier =
                Modifier
                    .size(44.dp)
                    .clip(RoundedCornerShape(12.dp))
                    .background(AumoColors.SurfaceElevated),
            contentAlignment = Alignment.Center,
        ) {
            TablerIcon(
                glyph = item.icon,
                tint = AumoColors.Primary,
                size = 22.dp,
            )
        }
        Spacer(modifier = Modifier.height(12.dp))
        Text(
            text = item.title,
            color = AumoColors.TextPrimary,
            fontWeight = FontWeight.SemiBold,
            fontSize = MaterialTheme.typography.bodyLarge.fontSize,
        )
        Spacer(modifier = Modifier.height(2.dp))
        Text(
            text = item.subtitle,
            color = AumoColors.TextMuted,
            fontSize = MaterialTheme.typography.bodySmall.fontSize,
        )
    }
}

private fun getDynamicGreeting(): String {
    val hour = JavaCalendar.getInstance().get(JavaCalendar.HOUR_OF_DAY)
    return when (hour) {
        in 4..11 -> "Good morning"
        in 12..16 -> "Good afternoon"
        in 17..20 -> "Good evening"
        else -> "Good night"
    }
}

object HomeIcons {
    val Dashboard = TablerIcons.LayoutDashboard
    val JournalEntry = TablerIcons.FilePlus
    val GeneralJournal = TablerIcons.Book
    val Periods = TablerIcons.Calendar
    val Coa = TablerIcons.GitFork
    val Reports = TablerIcons.ReportAnalytics
}
