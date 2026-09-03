package com.aumofinance.app.journal

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.coa.Account
import com.aumofinance.app.coa.AccountsResponse
import com.aumofinance.app.coa.CoaApi
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
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
    private val api = ApiClient.retrofit.create(JournalApi::class.java)
    private val coaApi = ApiClient.retrofit.create(CoaApi::class.java)

    companion object {
        val JOURNAL_TYPES = listOf("General", "Adjusting")
        private val DATE_ONLY_ISO = SimpleDateFormat("yyyy-MM-dd'T'00:00:00", Locale.US)
    }

    // --- State form (dibaca langsung oleh JournalEntryScreen) ---
    var entryId: Int? = null
        private set
    var journalType: String by mutableStateOf(JOURNAL_TYPES.first())
    var entryDate: Calendar by mutableStateOf(Calendar.getInstance())
    var transactionNumber: String by mutableStateOf("")
        private set
    var isLocked: Boolean by mutableStateOf(false)
        private set
    val lines = mutableStateListOf(JournalLineDraft(), JournalLineDraft())
    var accounts: List<Account> by mutableStateOf(emptyList())
        private set

    // --- Sinyal hasil operasi (diobservasi Activity untuk toast/navigasi) ---
    private val _errorMessage = MutableLiveData<String?>()
    val errorMessage: LiveData<String?> = _errorMessage

    private val _saveResult = MutableLiveData<CreateJournalEntryResponse?>()
    val saveResult: LiveData<CreateJournalEntryResponse?> = _saveResult

    private val _updateResult = MutableLiveData<Boolean?>()
    val updateResult: LiveData<Boolean?> = _updateResult

    // Dipanggil oleh Activity setelah menampilkan Toast/navigasi, supaya
    // sinyal ini tidak "nyangkut" dan terpicu ulang tiap recomposition
    // Compose (mis. tiap kali user mengetik di baris lain).
    fun clearError() { _errorMessage.value = null }
    fun clearSaveResult() { _saveResult.value = null }
    fun clearUpdateResult() { _updateResult.value = null }

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
        journalType = type
        if (entryId == null) refreshNextTransactionNumber()
    }

    fun setEntryDate(date: Calendar) {
        entryDate = date
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
        coaApi.list().enqueue(object : Callback<AccountsResponse> {
            override fun onResponse(call: Call<AccountsResponse>, response: Response<AccountsResponse>) {
                accounts = response.body()?.accounts?.filter { it.isActive } ?: emptyList()
            }
            override fun onFailure(call: Call<AccountsResponse>, t: Throwable) = Unit
        })
    }

    private fun refreshNextTransactionNumber() {
        val entryDateIso = DATE_ONLY_ISO.format(entryDate.time)
        api.nextTransactionNumber(journalType, entryDateIso).enqueue(object : Callback<NextTransactionNumberResponse> {
            override fun onResponse(call: Call<NextTransactionNumberResponse>, response: Response<NextTransactionNumberResponse>) {
                response.body()?.let { transactionNumber = it.transactionNumber }
            }
            override fun onFailure(call: Call<NextTransactionNumberResponse>, t: Throwable) = Unit
        })
    }

    private fun loadById(id: Int) {
        api.getById(id).enqueue(object : Callback<JournalEntryDetailResponse> {
            override fun onResponse(call: Call<JournalEntryDetailResponse>, response: Response<JournalEntryDetailResponse>) {
                response.body()?.entry?.let { bindExistingEntry(it) }
            }
            override fun onFailure(call: Call<JournalEntryDetailResponse>, t: Throwable) = Unit
        })
    }

    private fun bindExistingEntry(detail: JournalEntryDetail) {
        journalType = JOURNAL_TYPES.firstOrNull { it == detail.journalType } ?: JOURNAL_TYPES.first()
        transactionNumber = detail.transactionNumber
        isLocked = detail.isLocked
        runCatching {
            val parsed = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US).parse(detail.entryDate)
            parsed?.let { entryDate = Calendar.getInstance().apply { time = it } }
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
            _errorMessage.value = "Entri belum balance — total debit harus sama dengan total kredit."
            return
        }
        if (lines.any { it.accountId == null }) {
            _errorMessage.value = "Setiap baris harus memilih akun."
            return
        }

        val entryDateIso = DATE_ONLY_ISO.format(entryDate.time)
        val now = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US).format(Calendar.getInstance().time)
        val apiLines = lines.mapIndexed { index, l ->
            JournalLine(l.accountId!!, l.description.ifBlank { null }, l.debitAmount(), l.creditAmount(), index)
        }

        val id = entryId
        if (id == null) {
            create(CreateJournalEntryRequest(journalType, entryDateIso, now, apiLines))
        } else {
            update(id, UpdateJournalEntryRequest(journalType, entryDateIso, now, apiLines))
        }
    }

    private fun create(request: CreateJournalEntryRequest) {
        api.create(request).enqueue(object : Callback<CreateJournalEntryResponse> {
            override fun onResponse(call: Call<CreateJournalEntryResponse>, response: Response<CreateJournalEntryResponse>) {
                if (response.isSuccessful && response.body()?.success == true) {
                    _saveResult.value = response.body()
                } else {
                    _errorMessage.value = response.body()?.message ?: "Gagal menyimpan entri (${response.code()})"
                }
            }
            override fun onFailure(call: Call<CreateJournalEntryResponse>, t: Throwable) {
                _errorMessage.value = t.message ?: "Koneksi gagal"
            }
        })
    }

    private fun update(id: Int, request: UpdateJournalEntryRequest) {
        api.update(id, request).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) {
                if (response.isSuccessful && response.body()?.success == true) {
                    _updateResult.value = true
                } else {
                    _errorMessage.value = response.body()?.message ?: "Gagal memperbarui entri (${response.code()})"
                }
            }
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
                _errorMessage.value = t.message ?: "Koneksi gagal"
            }
        })
    }
}
