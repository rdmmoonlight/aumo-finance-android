package com.aumofinance.app.reports.journal

import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.widget.TextView
import android.widget.ToggleButton
import android.widget.Toast
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.journal.JournalApi
import com.aumofinance.app.journal.JournalEntryActivity
import com.aumofinance.app.journal.SimpleApiResponse
import com.aumofinance.app.network.ApiClient
import com.aumofinance.app.R
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

// Menampilkan seluruh entri (General + Adjusting) periode terpilih,
// dikelompokkan per tanggal. Tombol edit/delete disembunyikan secara default
// (baru muncul saat toggle "Edit" dinyalakan) — berbeda dari Adjusting
// Journal yang selalu menampilkannya.
class GeneralJournalReportActivity : AppCompatActivity() {
    private val viewModel: JournalReportViewModel by viewModels()
    private lateinit var adapter: JournalReportAdapter

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_general_journal_report)

        adapter = JournalReportAdapter(
            showActions = false,
            onEdit = { entry -> openEdit(entry.id) },
            onDelete = { entry -> confirmDelete(entry) }
        )
        findViewById<RecyclerView>(R.id.recyclerEntries).apply {
            layoutManager = LinearLayoutManager(this@GeneralJournalReportActivity)
            adapter = this@GeneralJournalReportActivity.adapter
        }

        findViewById<ToggleButton>(R.id.toggleEditMode).setOnCheckedChangeListener { _, isChecked ->
            adapter.setShowActions(isChecked)
        }

        viewModel.entries.observe(this) { adapter.submitEntries(it) }
        viewModel.selectedPeriodName.observe(this) { name ->
            findViewById<TextView>(R.id.textPeriodName).text = name ?: "Belum ada periode dipilih"
        }
        viewModel.loadGeneral()
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadGeneral()
    }

    private fun openEdit(entryId: Int) {
        startActivity(Intent(this, JournalEntryActivity::class.java).apply {
            putExtra(JournalEntryActivity.EXTRA_ENTRY_ID, entryId)
        })
    }

    private fun confirmDelete(entry: JournalReportEntry) {
        AlertDialog.Builder(this)
            .setTitle("Hapus Entri?")
            .setMessage("Entri \"${entry.transactionNumber}\" akan dihapus permanen. Lanjutkan?")
            .setPositiveButton("Hapus") { _, _ -> deleteEntry(entry.id) }
            .setNegativeButton("Batal", null)
            .show()
    }

    private fun deleteEntry(id: Int) {
        val api = ApiClient.retrofit.create(JournalApi::class.java)
        api.delete(id).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) {
                if (response.isSuccessful && response.body()?.success == true) {
                    viewModel.loadGeneral()
                } else {
                    Toast.makeText(this@GeneralJournalReportActivity, response.body()?.message ?: "Gagal menghapus entri", Toast.LENGTH_LONG).show()
                }
            }
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
                Toast.makeText(this@GeneralJournalReportActivity, t.message ?: "Koneksi gagal", Toast.LENGTH_LONG).show()
            }
        })
    }
}
