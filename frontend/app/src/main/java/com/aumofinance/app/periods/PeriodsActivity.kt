package com.aumofinance.app.periods

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import com.aumofinance.app.R

class PeriodsActivity : AppCompatActivity() {
    private val viewModel: PeriodsViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_periods)

        // TODO: RecyclerView + PeriodsAdapter (Aktif/Ditutup badge, tombol "Buka Periode Baru")
        viewModel.periods.observe(this) { /* bind ke adapter */ }
        viewModel.load()
    }
}
