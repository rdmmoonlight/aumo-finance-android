package com.aumofinance.app.journal

import android.app.DatePickerDialog
import android.os.Bundle
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.Spinner
import android.widget.TextView
import android.widget.Toast
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.coa.Account
import com.aumofinance.app.coa.AccountsResponse
import com.aumofinance.app.coa.CoaApi
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.network.ApiClient
import com.aumofinance.app.R
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import java.text.SimpleDateFormat
import java.util.Calendar
import java.util.Locale

// Form input satu Journal Entry (create atau edit, tergantung apakah
// EXTRA_ENTRY_ID diberikan). EntryDate (tanggal manual) dan CreatedAt/UpdatedAt
// (waktu lokal perangkat) diformat TANPA info zona waktu (mis.
// "2026-08-28T14:30:00", bukan dengan sufiks "Z" atau offset) — backend
// hanya me-relabel nilai itu sebagai UTC apa adanya (DateTime.SpecifyKind),
// bukan mengonversi, jadi nilai jam dinding perangkat harus sampai persis
// sama tanpa digeser (riwayat bug lama: tanggal mundur 1 hari).
// Tidak ada field periodId — backend hanya menolak entri yang EntryDate-nya
// jatuh di periode yang sudah Closed (lihat PeriodLock.IsDateLocked).
class JournalEntryActivity : AppCompatActivity() {

    companion object {
        const val EXTRA_ENTRY_ID = "extra_entry_id"
        private val ISO_LOCAL = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US)
        private val DATE_ONLY_DISPLAY = SimpleDateFormat("dd MMM yyyy", Locale("in", "ID"))
        private val DATE_ONLY_ISO = SimpleDateFormat("yyyy-MM-dd'T'00:00:00", Locale.US)
    }

    private val viewModel: JournalEntryViewModel by viewModels()
    private var entryId: Int? = null
    private var selectedDate: Calendar = Calendar.getInstance()

    private val journalTypes = listOf("General", "Adjusting")
    private val lines = mutableListOf(JournalLineDraft(), JournalLineDraft())
    private lateinit var lineAdapter: JournalLineAdapter
    private var accounts: List<Account> = emptyList()

    private lateinit var buttonEntryDate: Button
    private lateinit var spinnerJournalType: Spinner
    private lateinit var textTotals: TextView
    private lateinit var textBalancedBadge: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_journal_entry)

        entryId = intent.getIntExtra(EXTRA_ENTRY_ID, -1).takeIf { it != -1 }

        buttonEntryDate = findViewById(R.id.buttonEntryDate)
        spinnerJournalType = findViewById(R.id.spinnerJournalType)
        textTotals = findViewById(R.id.textTotals)
        textBalancedBadge = findViewById(R.id.textBalancedBadge)

        spinnerJournalType.adapter = ArrayAdapter(this, android.R.layout.simple_spinner_dropdown_item, journalTypes)
        updateDateButtonLabel()
        buttonEntryDate.setOnClickListener { showDatePicker() }

        lineAdapter = JournalLineAdapter(lines, accounts) { updateTotals() }
        findViewById<RecyclerView>(R.id.recyclerLines).apply {
            layoutManager = LinearLayoutManager(this@JournalEntryActivity)
            adapter = lineAdapter
        }

        findViewById<Button>(R.id.buttonAddLine).setOnClickListener {
            lineAdapter.addLine()
            updateTotals()
        }

        findViewById<Button>(R.id.buttonSaveEntry).setOnClickListener { save() }

        viewModel.entry.observe(this) { detail -> detail?.let { bindExistingEntry(it) } }
        viewModel.errorMessage.observe(this) { message ->
            message?.let { Toast.makeText(this, it, Toast.LENGTH_LONG).show() }
        }
        viewModel.saveResult.observe(this) {
            Toast.makeText(this, "Entri tersimpan", Toast.LENGTH_SHORT).show()
            finish()
        }

        loadActiveAccounts()
        entryId?.let { viewModel.loadById(it) }
        updateTotals()
    }

    private fun loadActiveAccounts() {
        val coaApi = ApiClient.retrofit.create(CoaApi::class.java)
        coaApi.list().enqueue(object : Callback<AccountsResponse> {
            override fun onResponse(call: Call<AccountsResponse>, response: Response<AccountsResponse>) {
                accounts = response.body()?.accounts?.filter { it.isActive } ?: emptyList()
                lineAdapter.setAccounts(accounts)
            }
            override fun onFailure(call: Call<AccountsResponse>, t: Throwable) = Unit
        })
    }

    private fun bindExistingEntry(detail: JournalEntryDetail) {
        spinnerJournalType.setSelection(journalTypes.indexOf(detail.journalType).coerceAtLeast(0))
        lines.clear()
        detail.lines.forEach { l ->
            lines.add(JournalLineDraft(l.accountId, l.lineDescription ?: "", formatAmount(l.debit), formatAmount(l.credit)))
        }
        if (lines.isEmpty()) lines.add(JournalLineDraft())
        lineAdapter.notifyDataSetChanged()
        updateTotals()
    }

    // TODO: ganti ke format ribuan yang lebih rapi saat menampilkan ulang nilai
    // existing (saat ini toString() Double bisa muncul sebagai "150000.0").
    private fun formatAmount(value: Double): String = if (value == 0.0) "" else value.toString()

    private fun showDatePicker() {
        DatePickerDialog(
            this,
            { _, year, month, day ->
                selectedDate.set(year, month, day)
                updateDateButtonLabel()
            },
            selectedDate.get(Calendar.YEAR),
            selectedDate.get(Calendar.MONTH),
            selectedDate.get(Calendar.DAY_OF_MONTH)
        ).show()
    }

    private fun updateDateButtonLabel() {
        buttonEntryDate.text = DATE_ONLY_DISPLAY.format(selectedDate.time)
    }

    private fun updateTotals() {
        val totalDebit = lines.sumOf { it.debitAmount() }
        val totalCredit = lines.sumOf { it.creditAmount() }
        textTotals.text = "Debit: ${CurrencyFormatter.format(totalDebit)}   Kredit: ${CurrencyFormatter.format(totalCredit)}"

        val balanced = totalDebit > 0 && totalDebit == totalCredit
        textBalancedBadge.text = if (balanced) "Balanced" else "Unbalanced"
        textBalancedBadge.setTextColor(
            resources.getColor(if (balanced) R.color.colorGood else R.color.colorBad, theme)
        )
    }

    private fun save() {
        val totalDebit = lines.sumOf { it.debitAmount() }
        val totalCredit = lines.sumOf { it.creditAmount() }
        if (totalDebit == 0.0 || totalDebit != totalCredit) {
            Toast.makeText(this, "Entri belum balance — total debit harus sama dengan total kredit.", Toast.LENGTH_LONG).show()
            return
        }
        if (lines.any { it.accountId == null }) {
            Toast.makeText(this, "Setiap baris harus memilih akun.", Toast.LENGTH_LONG).show()
            return
        }

        val journalType = journalTypes[spinnerJournalType.selectedItemPosition]
        val entryDateIso = DATE_ONLY_ISO.format(selectedDate.time)
        val now = ISO_LOCAL.format(Calendar.getInstance().time)
        val apiLines = lines.mapIndexed { index, l ->
            JournalLine(l.accountId!!, l.description.ifBlank { null }, l.debitAmount(), l.creditAmount(), index)
        }

        val id = entryId
        if (id == null) {
            viewModel.create(CreateJournalEntryRequest(journalType, entryDateIso, now, apiLines))
        } else {
            viewModel.update(id, UpdateJournalEntryRequest(journalType, entryDateIso, now, apiLines))
        }
    }
}
