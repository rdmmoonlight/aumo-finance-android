package com.aumofinance.app.periods

import android.app.AlertDialog
import android.os.Bundle
import android.widget.EditText
import android.widget.LinearLayout
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.R

class PeriodsActivity : AppCompatActivity() {
    private val viewModel: PeriodsViewModel by viewModels()
    private lateinit var adapter: PeriodsAdapter

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_periods)

        adapter = PeriodsAdapter(
            items = emptyList(),
            selectedPeriodId = null,
            onSelect = { period -> viewModel.select(period.id) },
            onClose = { period -> confirmClose(period) }
        )

        findViewById<RecyclerView>(R.id.recyclerPeriods).apply {
            layoutManager = LinearLayoutManager(this@PeriodsActivity)
            adapter = this@PeriodsActivity.adapter
        }

        findViewById<android.widget.Button>(R.id.buttonOpenPeriod).setOnClickListener {
            showOpenPeriodDialog()
        }

        viewModel.periods.observe(this) { periods ->
            adapter.submitList(periods, viewModel.selectedPeriodId.value)
        }
        viewModel.selectedPeriodId.observe(this) { selectedId ->
            adapter.submitList(viewModel.periods.value ?: emptyList(), selectedId)
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }

    private fun confirmClose(period: Period) {
        AlertDialog.Builder(this)
            .setTitle("Tutup Periode?")
            .setMessage("Periode \"${period.periodName}\" akan ditutup dan tidak bisa menerima entri baru lagi. Lanjutkan?")
            .setPositiveButton("Tutup") { _, _ -> viewModel.close(period.id) }
            .setNegativeButton("Batal", null)
            .show()
    }

    private fun showOpenPeriodDialog() {
        val container = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(48, 24, 48, 0)
        }
        val inputName = EditText(this).apply { hint = "Nama Periode (mis. Januari 2026)" }
        val inputStart = EditText(this).apply { hint = "Tanggal Mulai (yyyy-MM-dd)" }
        val inputEnd = EditText(this).apply { hint = "Tanggal Selesai (yyyy-MM-dd)" }
        // TODO: ganti 3 EditText tanggal/nama ini dengan DatePickerDialog + validasi format,
        // ini masih input manual sekadar supaya alur buka periode berfungsi.
        container.addView(inputName)
        container.addView(inputStart)
        container.addView(inputEnd)

        AlertDialog.Builder(this)
            .setTitle("Buka Periode Baru")
            .setView(container)
            .setPositiveButton("Buka") { _, _ ->
                viewModel.open(
                    CreatePeriodRequest(
                        periodName = inputName.text.toString(),
                        startDate = inputStart.text.toString(),
                        endDate = inputEnd.text.toString()
                    )
                )
            }
            .setNegativeButton("Batal", null)
            .show()
    }
}
