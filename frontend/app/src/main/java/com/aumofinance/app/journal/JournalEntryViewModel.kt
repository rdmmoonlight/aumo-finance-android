package com.aumofinance.app.journal

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

// Menangani satu form Journal Entry (create/edit/delete satu entri).
// Daftar entri (halaman General Journal / Adjusting Journal) ada di
// reports.journal.JournalReportViewModel, bukan di sini — mengikuti pemisahan
// endpoint yang sama di aumo-finance-web (journal-entry vs journal-entries).
class JournalEntryViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(JournalApi::class.java)

    private val _entry = MutableLiveData<JournalEntryDetail?>()
    val entry: LiveData<JournalEntryDetail?> = _entry

    private val _saveResult = MutableLiveData<CreateJournalEntryResponse?>()
    val saveResult: LiveData<CreateJournalEntryResponse?> = _saveResult

    // Sinyal sukses khusus untuk update (edit). Terpisah dari saveResult
    // (yang hanya diisi oleh create) karena PUT tidak mengembalikan
    // CreateJournalEntryResponse — sebelumnya update() sukses tidak pernah
    // memberi tahu Activity, sehingga form terlihat "tidak tersimpan"
    // walaupun request ke server sebenarnya berhasil.
    private val _updateResult = MutableLiveData<Boolean?>()
    val updateResult: LiveData<Boolean?> = _updateResult

    private val _errorMessage = MutableLiveData<String?>()
    val errorMessage: LiveData<String?> = _errorMessage

    fun loadById(id: Int) {
        api.getById(id).enqueue(object : Callback<JournalEntryDetailResponse> {
            override fun onResponse(call: Call<JournalEntryDetailResponse>, response: Response<JournalEntryDetailResponse>) {
                _entry.value = response.body()?.entry
            }
            override fun onFailure(call: Call<JournalEntryDetailResponse>, t: Throwable) {
                _entry.value = null
            }
        })
    }

    fun create(request: CreateJournalEntryRequest) {
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

    fun update(id: Int, request: UpdateJournalEntryRequest) {
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

    fun delete(id: Int, onDeleted: () -> Unit) {
        api.delete(id).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) {
                if (response.isSuccessful && response.body()?.success == true) onDeleted()
                else _errorMessage.value = response.body()?.message ?: "Gagal menghapus entri (${response.code()})"
            }
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
                _errorMessage.value = t.message ?: "Koneksi gagal"
            }
        })
    }
}
