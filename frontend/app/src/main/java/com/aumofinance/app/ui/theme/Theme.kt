package com.aumofinance.app.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

// Palet Matte Black + Ningrat Purple — nilai sama persis dengan
// app/src/main/res/values/themes.xml (colorPrimary, colorBackground, dst).
// Disalin manual (bukan dibaca dari resource) karena ColorScheme Compose
// butuh tipe androidx.compose.ui.graphics.Color, bukan Int resource.
object AumoColors {
    val Primary = Color(0xFF523363)
    val Background = Color(0xFF0A0A0A)
    val Surface = Color(0xFF141014)
    val SurfaceElevated = Color(0xFF1E121F)
    val Good = Color(0xFF4FA36A)
    val Bad = Color(0xFFD7192F)

    val TextPrimary = Color(0xFFFFFFFF)
    val TextSecondary = Color(0xFFD8D8D8)
    val TextMuted = Color(0xFF9C8FA6)
    val Border = Color(0xFF4A2E59)
}

private val AumoDarkScheme = darkColorScheme(
    primary = AumoColors.Primary,
    onPrimary = AumoColors.TextPrimary,
    background = AumoColors.Background,
    onBackground = AumoColors.TextPrimary,
    surface = AumoColors.Surface,
    onSurface = AumoColors.TextPrimary,
    surfaceVariant = AumoColors.SurfaceElevated,
    onSurfaceVariant = AumoColors.TextSecondary,
    error = AumoColors.Bad,
    outline = AumoColors.Border
)

@Composable
fun AumoTheme(content: @Composable () -> Unit) {
    // Aplikasi ini satu tema saja (dark, matte) — tidak mengikuti tema sistem.
    MaterialTheme(
        colorScheme = AumoDarkScheme,
        content = content
    )
}
