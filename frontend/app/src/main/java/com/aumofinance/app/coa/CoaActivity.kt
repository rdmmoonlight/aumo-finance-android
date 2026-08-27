package com.aumofinance.app.coa

import android.os.Bundle
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.R

class CoaActivity : AppCompatActivity() {
    private val viewModel: CoaViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_coa)

        // TODO: search bar + toggle Edit mode + RecyclerView (badge Aktif/Nonaktif, saldo)
        viewModel.accounts.observe(this) { /* bind ke adapter */ }
        viewModel.load()
    }
}
