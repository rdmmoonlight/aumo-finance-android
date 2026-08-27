package com.aumofinance.app.journal

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Catatan: EntryDate (tanggal manual) dan CreatedAt (waktu lokal device) dikirim terpisah,
// dan CreatedAt harus dikirim dengan DateTimeKind.Unspecified agar tidak digeser oleh
// konversi zona waktu server (lihat riwayat bug di aumo-finance-android lama).
class JournalEntryActivity : AppCompatActivity() {
    private val viewModel: JournalEntryViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_journal_entry)

        // TODO: form baris debit/kredit dinamis, badge Balanced/Unbalanced,
        // input Rupiah dengan pemisah ribuan (bukan type=number)
        viewModel.entries.observe(this) { /* bind ke RecyclerView, dikelompokkan per tanggal */ }
        viewModel.load()
    }
}
