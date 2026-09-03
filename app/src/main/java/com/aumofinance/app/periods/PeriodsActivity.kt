package com.aumofinance.app.periods

import android.app.AlertDialog
import android.os.Bundle
import android.text.InputType
import android.widget.ArrayAdapter
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.Spinner
import android.widget.TextView
import android.widget.Toast
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.R
import java.util.Calendar

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
            // Selalu ambil info kondisi terbaru dulu (apakah sudah ada akun
            // permanen dari periode yang pernah ditutup atau belum) sebelum
            // menampilkan dialog, karena kondisi ini bisa berubah kapan saja.
            viewModel.loadOpenPeriodInfo()
        }

        viewModel.periods.observe(this) { periods ->
            adapter.submitList(periods, viewModel.selectedPeriodId.value)
        }
        viewModel.selectedPeriodId.observe(this) { selectedId ->
            adapter.submitList(viewModel.periods.value ?: emptyList(), selectedId)
        }
        viewModel.openPeriodInfo.observe(this) { info ->
            if (info != null) showOpenPeriodDialog(info)
        }
        viewModel.actionResult.observe(this) { result ->
            if (result != null) {
                Toast.makeText(this, result.message, Toast.LENGTH_LONG).show()
            }
        }

        viewModel.load()
    }

    override fun onResume() {
        super.onResume()
        viewModel.load()
    }

    private fun confirmClose(period: Period) {
        AlertDialog.Builder(this)
            .setTitle("Close Period?")
            .setMessage("Period \"${period.periodName}\" will be closed and cannot accept new entries anymore. Continue?")
            .setPositiveButton("Close") { _, _ -> viewModel.close(period.id) }
            .setNegativeButton("Cancel", null)
            .show()
    }

    // Kondisi 1: belum ada periode yang pernah ditutup (belum ada akun
    // permanen sama sekali) -> user WAJIB mendaftarkan akun Cash, Bank, dan
    // Retained Earnings baru, plus (opsional) saldo awal Cash/Bank.
    //
    // Kondisi 2: sudah ada periode yang pernah ditutup (akun permanen sudah
    // ada) -> user tinggal MEMILIH akun Cash, Bank, dan Retained Earnings
    // yang sudah ada; saldo otomatis nyambung dari ledger periode
    // sebelumnya, tanpa perlu input saldo awal lagi.
    private fun showOpenPeriodDialog(info: OpenPeriodInfoResponse) {
        val container = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(48, 24, 48, 0)
        }

        val now = Calendar.getInstance()
        val inputMonth = EditText(this).apply {
            hint = "Month (1-12)"
            inputType = InputType.TYPE_CLASS_NUMBER
            setText((now.get(Calendar.MONTH) + 1).toString())
        }
        val inputYear = EditText(this).apply {
            hint = "Year (e.g., 2026)"
            inputType = InputType.TYPE_CLASS_NUMBER
            setText(now.get(Calendar.YEAR).toString())
        }
        container.addView(inputMonth)
        container.addView(inputYear)

        val hasExisting = info.hasExistingPermanentAccounts

        // --- Kondisi 2: sudah ada periode yang pernah ditutup ---
        var spinnerCash: Spinner? = null
        var spinnerBank: Spinner? = null
        var spinnerRetained: Spinner? = null

        // --- Kondisi 1: belum ada periode yang pernah ditutup ---
        var cashCode: EditText? = null
        var cashName: EditText? = null
        var cashBalance: EditText? = null
        var bankCode: EditText? = null
        var bankName: EditText? = null
        var bankBalance: EditText? = null
        var retainedCode: EditText? = null
        var retainedName: EditText? = null

        if (hasExisting) {
            container.addView(TextView(this).apply {
                text = "A closed period already exists. Select which accounts to carry forward:"
                setPadding(0, 24, 0, 8)
            })

            val cashBankLabels = info.availableCashAndBankAccounts.map { "${it.referenceNumber} - ${it.accountName}" }
            val retainedLabels = info.availableRetainedEarningsAccounts.map { "${it.referenceNumber} - ${it.accountName}" }

            container.addView(TextView(this).apply { text = "Cash Account" })
            spinnerCash = Spinner(this).apply {
                adapter = ArrayAdapter(this@PeriodsActivity, android.R.layout.simple_spinner_dropdown_item, cashBankLabels)
            }
            container.addView(spinnerCash)

            container.addView(TextView(this).apply { text = "Bank Account" })
            spinnerBank = Spinner(this).apply {
                adapter = ArrayAdapter(this@PeriodsActivity, android.R.layout.simple_spinner_dropdown_item, cashBankLabels)
            }
            container.addView(spinnerBank)

            container.addView(TextView(this).apply { text = "Retained Earnings Account" })
            spinnerRetained = Spinner(this).apply {
                adapter = ArrayAdapter(this@PeriodsActivity, android.R.layout.simple_spinner_dropdown_item, retainedLabels)
            }
            container.addView(spinnerRetained)
        } else {
            container.addView(TextView(this).apply {
                text = "No period has been closed yet. Register your permanent accounts and opening balances:"
                setPadding(0, 24, 0, 8)
            })

            container.addView(TextView(this).apply { text = "Cash Account" })
            cashCode = EditText(this).apply { hint = "Reference Code (e.g., 101)"; inputType = InputType.TYPE_CLASS_NUMBER }
            cashName = EditText(this).apply { hint = "Account Name (e.g., Cash)" }
            cashBalance = EditText(this).apply { hint = "Opening Balance"; inputType = InputType.TYPE_CLASS_NUMBER or InputType.TYPE_NUMBER_FLAG_DECIMAL }
            container.addView(cashCode)
            container.addView(cashName)
            container.addView(cashBalance)

            container.addView(TextView(this).apply { text = "Bank Account"; setPadding(0, 16, 0, 0) })
            bankCode = EditText(this).apply { hint = "Reference Code (e.g., 102)"; inputType = InputType.TYPE_CLASS_NUMBER }
            bankName = EditText(this).apply { hint = "Account Name (e.g., Bank)" }
            bankBalance = EditText(this).apply { hint = "Opening Balance"; inputType = InputType.TYPE_CLASS_NUMBER or InputType.TYPE_NUMBER_FLAG_DECIMAL }
            container.addView(bankCode)
            container.addView(bankName)
            container.addView(bankBalance)

            container.addView(TextView(this).apply { text = "Retained Earnings Account"; setPadding(0, 16, 0, 0) })
            retainedCode = EditText(this).apply { hint = "Reference Code (e.g., 301)"; inputType = InputType.TYPE_CLASS_NUMBER }
            retainedName = EditText(this).apply { hint = "Account Name (e.g., Retained Earnings)" }
            container.addView(retainedCode)
            container.addView(retainedName)
        }

        AlertDialog.Builder(this)
            .setTitle("Open New Period")
            .setView(container)
            .setPositiveButton("Open") { _, _ ->
                val month = inputMonth.text.toString().toIntOrNull()
                val year = inputYear.text.toString().toIntOrNull()
                if (month == null || year == null) {
                    Toast.makeText(this, "Please enter a valid month and year.", Toast.LENGTH_LONG).show()
                    return@setPositiveButton
                }

                if (hasExisting) {
                    val cashSelection = info.availableCashAndBankAccounts.getOrNull(spinnerCash?.selectedItemPosition ?: -1)
                    val bankSelection = info.availableCashAndBankAccounts.getOrNull(spinnerBank?.selectedItemPosition ?: -1)
                    val retainedSelection = info.availableRetainedEarningsAccounts.getOrNull(spinnerRetained?.selectedItemPosition ?: -1)
                    if (cashSelection == null || bankSelection == null || retainedSelection == null) {
                        Toast.makeText(this, "Please select Cash, Bank, and Retained Earnings accounts.", Toast.LENGTH_LONG).show()
                        return@setPositiveButton
                    }
                    viewModel.open(
                        CreatePeriodRequest(
                            month = month,
                            year = year,
                            setupMode = CreatePeriodRequest.MODE_LOAD_EXISTING,
                            cashAccountId = cashSelection.id,
                            bankAccountId = bankSelection.id,
                            retainedEarningsAccountId = retainedSelection.id
                        )
                    )
                } else {
                    viewModel.open(
                        CreatePeriodRequest(
                            month = month,
                            year = year,
                            setupMode = CreatePeriodRequest.MODE_CREATE_NEW,
                            cashAccountCode = cashCode?.text?.toString(),
                            cashAccountName = cashName?.text?.toString(),
                            cashBalance = cashBalance?.text?.toString()?.toDoubleOrNull(),
                            bankAccountCode = bankCode?.text?.toString(),
                            bankAccountName = bankName?.text?.toString(),
                            bankBalance = bankBalance?.text?.toString()?.toDoubleOrNull(),
                            retainedEarningsAccountCode = retainedCode?.text?.toString(),
                            retainedEarningsAccountName = retainedName?.text?.toString()
                        )
                    )
                }
            }
            .setNegativeButton("Cancel", null)
            .show()
    }
}
