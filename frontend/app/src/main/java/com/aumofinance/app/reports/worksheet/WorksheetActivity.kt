package com.aumofinance.app.reports.worksheet

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Footer worksheet WAJIB menampilkan 3 baris total standar akuntansi:
// 1) Total (sebelum plug), 2) Laba/Rugi Bersih (plug ke Neraca), 3) Total Akhir (setelah plug).
// Istilah "plug" tidak ditampilkan ke pengguna — pakai bahasa Indonesia biasa
// ("Laba/Rugi Bersih (dipindahkan ke Neraca)" / "Total Akhir").
class WorksheetActivity : AppCompatActivity() {
    private val viewModel: WorksheetViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_worksheet)

        // TODO: sinkronkan TopHeader.PeriodText (bukan kartu periode terpisah di halaman),
        // font dan padding mengikuti rhythm Income Statement (12px label, 13px data)
        viewModel.report.observe(this) { /* bind tabel 5 pasang kolom + 3 baris footer */ }
        viewModel.load()
    }
}
