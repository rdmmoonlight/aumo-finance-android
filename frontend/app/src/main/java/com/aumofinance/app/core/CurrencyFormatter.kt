package com.aumofinance.app.core

import java.text.NumberFormat
import java.util.Locale

// Format Rupiah konsisten di seluruh app: "Rp 1.234.567" (tanpa desimal,
// pemisah ribuan titik sesuai locale Indonesia). Nilai negatif ditampilkan
// dengan tanda minus di depan "Rp", mis. "-Rp 50.000".
object CurrencyFormatter {
    private val formatter: NumberFormat = NumberFormat.getNumberInstance(Locale("in", "ID")).apply {
        maximumFractionDigits = 0
    }

    fun format(amount: Double): String {
        val rounded = Math.round(amount)
        val prefix = if (rounded < 0) "-Rp " else "Rp "
        return prefix + formatter.format(Math.abs(rounded))
    }
}
