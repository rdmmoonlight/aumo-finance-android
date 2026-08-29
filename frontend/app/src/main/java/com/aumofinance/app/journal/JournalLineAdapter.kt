package com.aumofinance.app.journal

import android.text.Editable
import android.text.TextWatcher
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import android.widget.EditText
import android.widget.ImageButton
import android.widget.Spinner
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.coa.Account
import com.aumofinance.app.R

// RecyclerView baris debit/kredit yang bisa diedit. Setiap perubahan (akun,
// deskripsi, angka) langsung ditulis balik ke JournalLineDraft yang sama, dan
// memicu onLinesChanged supaya Activity bisa hitung ulang total Debit/Kredit.
class JournalLineAdapter(
    private val lines: MutableList<JournalLineDraft>,
    private var accounts: List<Account>,
    private val onLinesChanged: () -> Unit
) : RecyclerView.Adapter<JournalLineAdapter.ViewHolder>() {

    fun setAccounts(newAccounts: List<Account>) {
        accounts = newAccounts
        notifyDataSetChanged()
    }

    fun addLine() {
        lines.add(JournalLineDraft())
        notifyItemInserted(lines.size - 1)
    }

    inner class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val spinnerAccount: Spinner = view.findViewById(R.id.spinnerAccount)
        val inputDescription: EditText = view.findViewById(R.id.inputDescription)
        val inputDebit: EditText = view.findViewById(R.id.inputDebit)
        val inputCredit: EditText = view.findViewById(R.id.inputCredit)
        val buttonRemove: ImageButton = view.findViewById(R.id.buttonRemoveLine)
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_journal_line, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val line = lines[position]
        val accountLabels = accounts.map { "${it.referenceNumber} - ${it.accountName}" }
        holder.spinnerAccount.adapter = ArrayAdapter(
            holder.itemView.context, android.R.layout.simple_spinner_dropdown_item, accountLabels
        )

        val selectedIndex = accounts.indexOfFirst { it.id == line.accountId }
        if (selectedIndex >= 0) holder.spinnerAccount.setSelection(selectedIndex)

        holder.spinnerAccount.setOnItemSelectedListener(object : android.widget.AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: android.widget.AdapterView<*>?, view: View?, pos: Int, id: Long) {
                line.accountId = accounts.getOrNull(pos)?.id
            }
            override fun onNothingSelected(parent: android.widget.AdapterView<*>?) = Unit
        })

        holder.inputDescription.removeTextChangedListenerSafely()
        holder.inputDescription.setText(line.description)
        holder.inputDescription.attachWatcher { line.description = it }

        holder.inputDebit.removeTextChangedListenerSafely()
        holder.inputDebit.setText(line.debit)
        holder.inputDebit.attachWatcher { line.debit = it; onLinesChanged() }

        holder.inputCredit.removeTextChangedListenerSafely()
        holder.inputCredit.setText(line.credit)
        holder.inputCredit.attachWatcher { line.credit = it; onLinesChanged() }

        holder.buttonRemove.setOnClickListener {
            val currentPos = holder.bindingAdapterPosition
            if (currentPos != RecyclerView.NO_POSITION && lines.size > 1) {
                lines.removeAt(currentPos)
                notifyItemRemoved(currentPos)
                onLinesChanged()
            }
        }
    }

    override fun getItemCount(): Int = lines.size

    // EditText di RecyclerView dipakai ulang antar baris — tag dipakai untuk
    // menyimpan/menghapus TextWatcher sebelumnya supaya tidak menempel ke
    // baris yang salah setelah recycle.
    private fun EditText.removeTextChangedListenerSafely() {
        (tag as? TextWatcher)?.let { removeTextChangedListener(it) }
        tag = null
    }

    private fun EditText.attachWatcher(onChanged: (String) -> Unit) {
        val watcher = object : TextWatcher {
            override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) = Unit
            override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) = Unit
            override fun afterTextChanged(s: Editable?) = onChanged(s?.toString() ?: "")
        }
        tag = watcher
        addTextChangedListener(watcher)
    }
}
