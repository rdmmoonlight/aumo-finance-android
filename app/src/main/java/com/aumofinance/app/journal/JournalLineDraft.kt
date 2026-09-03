package com.aumofinance.app.journal

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue

// Representasi satu baris form selagi diedit user (sebelum dikirim ke API).
// accountId null berarti user belum memilih akun untuk baris ini.
// Field berbasis mutableStateOf agar tiap baris di Compose recompose
// sendiri saat diketik, tanpa perlu notifyDataSetChanged ala RecyclerView.
// debit/credit menyimpan STRING DIGIT MENTAH (tanpa titik pemisah ribuan) —
// format tampilan (mis. "150.000") dihitung ulang di layer UI.
class JournalLineDraft(
    accountId: Int? = null,
    description: String = "",
    debit: String = "",
    credit: String = ""
) {
    var accountId: Int? by mutableStateOf(accountId)
    var description: String by mutableStateOf(description)
    var debit: String by mutableStateOf(debit)
    var credit: String by mutableStateOf(credit)

    fun debitAmount(): Double = debit.toDoubleOrNull() ?: 0.0
    fun creditAmount(): Double = credit.toDoubleOrNull() ?: 0.0
}
