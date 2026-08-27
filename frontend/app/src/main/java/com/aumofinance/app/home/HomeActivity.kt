package com.aumofinance.app.home

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

// Landing page pasca-login: ringkasan cepat + navigasi ke Dashboard/Periods/COA/Journal Entry.
class HomeActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_home)
        // TODO fase berikutnya: hubungkan tombol menu ke masing-masing Activity
    }
}
