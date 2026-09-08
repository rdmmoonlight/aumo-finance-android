package com.aumofinance.app.journal

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.coa.Account
import com.aumofinance.app.coa.AccountsResponse
import com.aumofinance.app.coa.CoaApi
import io.ktor.client.call.body
import io.ktor.http.isSuccess
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.Calendar
import java.util.Locale

// Menangani satu form Journal Entry (create/edit satu entri) SEKALIGUS
// state Compose-nya (journal type, tanggal, nomor transaksi, baris, daftar
// akun) — sebelumnya state form ini hidup di JournalEntryActivity, dipindah
// ke sini supaya Activity cukup jadi host tipis untuk JournalEntryScreen.
// Daftar entri (halaman General Journal / Adjusting Journal) ada di
// reports.journal.JournalReportViewModel, bukan di sini — mengikuti pemisahan
// endpoint yang sama di aumo-finance-web (journal-entry vs journal-entries).
class JournalEntryViewModel : ViewModel() {
    private val api = JournalApi()
    private val coaApi = CoaApi()

    companion object {
        val JOURNAL_TYPES = listOf("General", "Adjusting")
        private val DATE_ONLY_ISO = SimpleDateFormat("yyyy-MM-dd'T'00:00:00", Locale.US)
    }

    // --- State form (dibaca langsung oleh JournalEntryScreen) ---
    var entryId: Int? = null
        private set
    private var _journalType: String by mutableStateOf(JOURNAL_TYPES.first())
    val journalType: String get() = _journalType

    private var _entryDate: Calendar by mutableStateOf(Calendar.getInstance())
    val entryDate: Calendar get() = _entryDate

    var transactionNumber: String by mutableStateOf("")
        private set
    var isLocked: Boolean by mutableStateOf(false)
        private set
    val lines = mutableStateListOf(JournalLineDraft(), JournalLineDraft())
    var accounts: List<Account> by mutableStateOf(emptyList())
        private set

    // --- Sinyal hasil operasi (dibaca langsung oleh JournalEntryScreen,
    // sama seperti field state form lain di atas — tidak pakai LiveData
    // supaya tidak perlu dependency androidx.compose.runtime:runtime-livedata
    // yang belum ada di build.gradle.kts) ---
    var errorMessage: String? by mutableStateOf(null)
        private set
    var saveResult: CreateJournalEntryResponse? by mutableStateOf(null)
        private set
    var updateResult: Boolean? by mutableStateOf(null)
        private set

    // Dipanggil oleh Activity setelah menampilkan Toast/navigasi, supaya
    // sinyal ini tidak "nyangkut" dan terpicu ulang tiap recomposition
    // Compose (mis. tiap kali user mengetik di baris lain).
    fun clearError() { errorMessage = null }
    fun clearSaveResult() { saveResult = null }
    fun clearUpdateResult() { updateResult = null }

    fun initFor(entryId: Int?) {
        this.entryId = entryId
        loadActiveAccounts()
        if (entryId != null) {
            loadById(entryId)
        } else {
            refreshNextTransactionNumber()
        }
    }

    fun setJournalType(type: String) {
        _journalType = type
        if (entryId == null) refreshNextTransactionNumber()
    }

    fun setEntryDate(date: Calendar) {
        _entryDate = date
        if (entryId == null) refreshNextTransactionNumber()
    }

    fun addLine() {
        lines.add(JournalLineDraft())
    }

    fun removeLine(line: JournalLineDraft) {
        if (lines.size > 1) lines.remove(line)
    }

    fun totalDebit(): Double = lines.sumOf { it.debitAmount() }
    fun totalCredit(): Double = lines.sumOf { it.creditAmount() }
    fun isBalanced(): Boolean = totalDebit() > 0 && totalDebit() == totalCredit()

    private fun loadActiveAccounts() {
        viewModelScope.launch {
            try {
                accounts = coaApi.list().body<AccountsResponse>().accounts.filter { it.isActive }
            } catch (t: Throwable) {
                // diam saja, sama seperti onFailure kosong sebelumnya
            }
        }
    }

    private fun refreshNextTransactionNumber() {
        val entryDateIso = DATE_ONLY_ISO.format(_entryDate.time)
        viewModelScope.launch {
            try {
                val body = api.nextTransactionNumber(_journalType, entryDateIso).body<NextTransactionNumberResponse>()
                transactionNumber = body.transactionNumber
            } catch (t: Throwable) {
                // diam saja, sama seperti onFailure kosong sebelumnya
            }
        }
    }

    private fun loadById(id: Int) {
        viewModelScope.launch {
            try {
                api.getById(id).body<JournalEntryDetailResponse>().entry?.let { bindExistingEntry(it) }
            } catch (t: Throwable) {
                // diam saja, sama seperti onFailure kosong sebelumnya
            }
        }
    }

    private fun bindExistingEntry(detail: JournalEntryDetail) {
        _journalType = JOURNAL_TYPES.firstOrNull { it == detail.journalType } ?: JOURNAL_TYPES.first()
        transactionNumber = detail.transactionNumber
        isLocked = detail.isLocked
        runCatching {
            val parsed = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US).parse(detail.entryDate)
            parsed?.let { _entryDate = Calendar.getInstance().apply { time = it } }
        }
        lines.clear()
        detail.lines.forEach { l ->
            lines.add(JournalLineDraft(l.accountId, l.lineDescription ?: "", formatAmount(l.debit), formatAmount(l.credit)))
        }
        if (lines.isEmpty()) lines.add(JournalLineDraft())
    }

    // Nilai existing dari API berupa Double (mis. 150000.0) — dikonversi ke
    // string digit mentah tanpa desimal, sesuai kontrak JournalLineDraft.
    private fun formatAmount(value: Double): String =
        if (value == 0.0) "" else Math.round(value).toString()

    fun save() {
        if (!isBalanced()) {
            errorMessage = "Entri belum balance — total debit harus sama dengan total kredit."
            return
        }
        if (lines.any { it.accountId == null }) {
            errorMessage = "Setiap baris harus memilih akun."
            return
        }

        val entryDateIso = DATE_ONLY_ISO.format(_entryDate.time)
        val now = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US).format(Calendar.getInstance().time)
        val apiLines = lines.mapIndexed { index, l ->
            JournalLine(l.accountId!!, l.description.ifBlank { null }, l.debitAmount(), l.creditAmount(), index)
        }

        val id = entryId
        if (id == null) {
            create(CreateJournalEntryRequest(_journalType, entryDateIso, now, apiLines))
        } else {
            update(id, UpdateJournalEntryRequest(_journalType, entryDateIso, now, apiLines))
        }
    }

    private fun create(request: CreateJournalEntryRequest) {
        viewModelScope.launch {
            try {
                val response = api.create(request)
                val body = response.body<CreateJournalEntryResponse>()
                if (response.status.isSuccess() && body.success) {
                    saveResult = body
                } else {
                    errorMessage = body.message.ifBlank { "Gagal menyimpan entri (${response.status.value})" }
                }
            } catch (t: Throwable) {
                errorMessage = t.message ?: "Koneksi gagal"
            }
        }
    }

    private fun update(id: Int, request: UpdateJournalEntryRequest) {
        viewModelScope.launch {
            try {
                val response = api.update(id, request)
                val body = response.body<SimpleApiResponse>()
                if (response.status.isSuccess() && body.success) {
                    updateResult = true
                } else {
                    errorMessage = body.message.ifBlank { "Gagal memperbarui entri (${response.status.value})" }
                }
            } catch (t: Throwable) {
                errorMessage = t.message ?: "Koneksi gagal"
            }
        }
    }
}
