package com.aumofinance.app.reports.financials

import android.content.Context
import android.view.Gravity
import android.widget.LinearLayout
import android.widget.TextView
import com.aumofinance.app.core.CurrencyFormatter

// Dipakai bersama oleh Income Statement, Retained Earnings, Financial
// Position, Cash Flow: satu baris "label (kiri) — nominal (kanan)", dengan
// opsi tebal untuk baris total/subtotal. Layout dibangun programatik karena
// tiap laporan punya jumlah baris dinamis (bergantung berapa akun ada).
object ReportRowBuilder {
    fun row(context: Context, label: String, amount: Double, bold: Boolean = false, indent: Boolean = false): LinearLayout {
        val row = LinearLayout(context).apply {
            orientation = LinearLayout.HORIZONTAL
            setPadding(0, 6, 0, 6)
        }
        val labelColor = if (bold) 0xFFFFFFFF.toInt() else 0xFF9C8FA6.toInt()
        val labelView = TextView(context).apply {
            text = if (indent) "    $label" else label
            textSize = 13f
            setTextColor(labelColor)
            if (bold) setTypeface(typeface, android.graphics.Typeface.BOLD)
            layoutParams = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
        }
        val amountView = TextView(context).apply {
            text = CurrencyFormatter.format(amount)
            textSize = 13f
            gravity = Gravity.END
            setTextColor(0xFFFFFFFF.toInt())
            if (bold) setTypeface(typeface, android.graphics.Typeface.BOLD)
        }
        row.addView(labelView)
        row.addView(amountView)
        return row
    }

    fun sectionTitle(context: Context, text: String): TextView = TextView(context).apply {
        this.text = text
        textSize = 14f
        setTextColor(0xFFFFFFFF.toInt())
        setTypeface(typeface, android.graphics.Typeface.BOLD)
        setPadding(0, 16, 0, 4)
    }

    fun divider(context: Context) = android.view.View(context).apply {
        layoutParams = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, 1).also { it.topMargin = 6 }
        setBackgroundColor(0xFF2A2A2A.toInt())
    }
}
