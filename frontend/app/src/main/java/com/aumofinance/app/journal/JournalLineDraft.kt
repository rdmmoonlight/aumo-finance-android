package com.aumofinance.app.journal

// Representasi satu baris form selagi diedit user (sebelum dikirim ke API).
// accountId null berarti user belum memilih akun untuk baris ini.
data class JournalLineDraft(
    var accountId: Int? = null,
    var description: String = "",
    var debit: String = "",
    var credit: String = ""
) {
    fun debitAmount(): Double = debit.toDoubleOrNull() ?: 0.0
    fun creditAmount(): Double = credit.toDoubleOrNull() ?: 0.0
}
