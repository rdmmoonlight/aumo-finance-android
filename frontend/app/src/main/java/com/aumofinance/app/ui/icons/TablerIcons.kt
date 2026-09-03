package com.aumofinance.app.ui.icons

import androidx.compose.foundation.layout.size
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.text.font.Font
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import com.aumofinance.app.R

// Font ikon Tabler Icons (versi 3.46.0, family "outline"/reguler — sama
// dengan yang dipakai di legacy-maui-reference, lihat glyph hex yang sama
// persis: &#xeb0b; = plus, &#xeb41; = trash, &#xea53; = calendar).
// Sumber: paket npm @tabler/icons-webfont, dist/fonts/tabler-icons.ttf.
private val TablerIconFont = FontFamily(Font(R.font.tabler_icons))

/** Kumpulan glyph Tabler Icons yang dipakai di seluruh app (Journal Entry
 * & Home). Satu-satunya sumber ikon Tabler — TIDAK memakai library
 * `br.com.devsrsouza.compose.icons` (sempat ditambahkan lalu memutus build
 * karena salah artifact ID/versi dan jcenter() sudah mati total sejak 2022)
 * supaya tidak ada dua sistem Tabler Icons yang saling tumpang tindih. */
object TablerIcons {
    const val Calendar = "\uea53"
    const val AlertTriangle = "\uea06"
    const val Plus = "\ueb0b"
    const val Selector = "\ueb1d"
    const val Trash = "\ueb41"
    const val Book = "\uea39"
    const val ChevronRight = "\uea61"
    const val FilePlus = "\ueaa0"
    const val GitFork = "\ueb8f"
    const val LayoutDashboard = "\uf02c"
    const val ReportAnalytics = "\ueecb"
    const val Settings = "\ueb20"
}

/** Menampilkan satu glyph Tabler Icons sebagai teks berikon. */
@Composable
fun TablerIcon(
    glyph: String,
    modifier: Modifier = Modifier,
    tint: Color = LocalContentColor.current,
    size: Dp = 18.dp
) {
    Text(
        text = glyph,
        modifier = modifier.size(size),
        color = tint,
        fontFamily = TablerIconFont,
        fontSize = with(LocalDensity.current) { size.toSp() }
    )
}
