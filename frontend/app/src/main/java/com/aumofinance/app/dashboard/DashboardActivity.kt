package com.aumofinance.app.dashboard

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

class DashboardActivity : AppCompatActivity() {
    private val viewModel: DashboardViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_dashboard)

        viewModel.summary.observe(this) { summary ->
            // TODO: bind summary ke TextView (Total Aset/Liabilitas/Ekuitas/Laba Bersih)
        }

        // TODO: ambil periodId aktif dari PeriodService/shared prefs, lalu viewModel.load(periodId)
    }
}
