package com.aumofinance.app.reports.journal

import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.widget.TextView
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

// Sama seperti General Journal, tapi backend sudah memfilter journalType
// "Adjusting" saja; selalu menampilkan tombol edit/delete (tanpa mode toggle
// Edit terpisah seperti General Journal).
class AdjustingJournalReportActivity : AppCompatActivity() {
    private val viewModel: JournalReportViewModel by viewModels()
    private lateinit var adapter: JournalReportAdapter

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_adjusting_journal_report)

        adapter = JournalReportAdapter(
            showActions = true,
            onEdit = { entry -> openEdit(entry.id) },
            onDelete = { entry -> confirmDelete(entry) }
        )
        findViewById<RecyclerView>(R.id.recyclerEntries).apply {
            layoutManager = LinearLayoutManager(this@AdjustingJournalReportActivity)
            adapter = this@AdjustingJournalReportActivity.adapter
        }

        viewModel.entries.observe(this) { adapter.submitEntries(it) }
        viewModel.selectedPeriodName.observe(this) { name ->
            val periodLabel = name ?: "Belum ada periode dipilih"
            findViewById<TextView>(R.id.textPeriodName).text = "$periodLabel · Nominal dalam Rupiah"
        }
        viewModel.loadAdjusting()
    }

    override fun onResume() {
        super.onResume()
        viewModel.loadAdjusting()
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
                    viewModel.loadAdjusting()
                } else {
                    Toast.makeText(this@AdjustingJournalReportActivity, response.body()?.message ?: "Gagal menghapus entri", Toast.LENGTH_LONG).show()
                }
            }
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
                Toast.makeText(this@AdjustingJournalReportActivity, t.message ?: "Koneksi gagal", Toast.LENGTH_LONG).show()
            }
        })
    }
}
