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

    // Dipakai di kotak input Debit/Kredit: menampilkan pemisah ribuan titik
    // sambil user mengetik, TANPA prefix "Rp". Input berupa string digit
    // mentah (hasil filter non-digit), output mis. "150000" -> "150.000".
    fun formatDigitsGrouped(rawDigits: String): String {
        if (rawDigits.isEmpty()) return ""
        val value = rawDigits.toLongOrNull() ?: return rawDigits
        return formatter.format(value)
    }
}
