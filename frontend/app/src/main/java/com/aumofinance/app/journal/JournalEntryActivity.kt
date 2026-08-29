package com.aumofinance.app.journal

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Form input satu Journal Entry (create atau edit, tergantung apakah
// EXTRA_ENTRY_ID diberikan). EntryDate (tanggal manual) dan CreatedAt (waktu
// lokal perangkat) dikirim terpisah dan HARUS diformat tanpa info zona waktu
// (mis. "2026-08-28T14:30:00", bukan dengan sufiks "Z" atau offset) — backend
// hanya me-relabel nilai itu sebagai UTC apa adanya (DateTime.SpecifyKind),
// bukan mengonversi, jadi nilai jam dinding perangkat harus sampai persis
// sama tanpa digeser (riwayat bug lama: tanggal mundur 1 hari karena Android
// sempat mengirim nilai dengan offset).
// Tidak ada field periodId — backend hanya menolak entri yang EntryDate-nya
// jatuh di periode yang sudah Closed (lihat PeriodLock.IsDateLocked).
class JournalEntryActivity : AppCompatActivity() {

    companion object {
        const val EXTRA_ENTRY_ID = "extra_entry_id"
    }

    private val viewModel: JournalEntryViewModel by viewModels()
    private var entryId: Int? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_journal_entry)

        entryId = intent.getIntExtra(EXTRA_ENTRY_ID, -1).takeIf { it != -1 }

        // TODO: form baris debit/kredit dinamis (minimal 2 baris efektif),
        // badge Balanced/Unbalanced, input Rupiah dengan pemisah ribuan
        // (bukan type=number), dropdown akun dari CoaApi (hanya yang aktif)
        viewModel.entry.observe(this) { detail -> /* isi form dari detail jika mode edit */ }
        viewModel.errorMessage.observe(this) { /* tampilkan Snackbar/Toast */ }
        viewModel.saveResult.observe(this) { /* tampilkan pesan sukses lalu finish() */ }

        entryId?.let { viewModel.loadById(it) }
    }
}
