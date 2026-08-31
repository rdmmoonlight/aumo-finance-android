package com.aumofinance.app.reports.journal

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageButton
import android.widget.LinearLayout
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R
import java.text.SimpleDateFormat
import java.util.Locale

private sealed class ReportRow
private data class DateHeaderRow(val label: String) : ReportRow()
private data class EntryRow(val entry: JournalReportEntry) : ReportRow()

// Menampilkan entri dikelompokkan per tanggal. Setiap entri dirender sebagai
// satu blok berisi header (nomor transaksi + tombol edit/delete) dan baris
// per JournalReportLine di dalamnya — baris kredit diindentasi satu tab dari
// debit (dua kolom teks: nama akun rata kiri, nominal rata kanan; baris
// kredit diberi indentasi spasi di depan nama akun).
class JournalReportAdapter(
    private var showActions: Boolean,
    private val onEdit: (JournalReportEntry) -> Unit,
    private val onDelete: (JournalReportEntry) -> Unit
) : RecyclerView.Adapter<RecyclerView.ViewHolder>() {

    private var rows: List<ReportRow> = emptyList()

    private val inputDateFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US)
    private val displayDateFormat = SimpleDateFormat("dd MMMM yyyy", Locale("in", "ID"))

    fun submitEntries(entries: List<JournalReportEntry>) {
        val grouped = entries.groupBy { it.entryDate.substringBefore("T") }
        val newRows = mutableListOf<ReportRow>()
        grouped.toSortedMap().forEach { (dateKey, entriesForDate) ->
            newRows.add(DateHeaderRow(formatDateLabel(dateKey)))
            entriesForDate.forEach { newRows.add(EntryRow(it)) }
        }
        rows = newRows
        notifyDataSetChanged()
    }

    fun setShowActions(value: Boolean) {
        showActions = value
        notifyDataSetChanged()
    }

    private fun formatDateLabel(dateKey: String): String = try {
        displayDateFormat.format(inputDateFormat.parse("${dateKey}T00:00:00")!!)
    } catch (e: Exception) {
        dateKey
    }

    override fun getItemViewType(position: Int): Int = when (rows[position]) {
        is DateHeaderRow -> 0
        is EntryRow -> 1
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RecyclerView.ViewHolder {
        val inflater = LayoutInflater.from(parent.context)
        return if (viewType == 0) {
            DateHeaderViewHolder(inflater.inflate(R.layout.item_journal_date_header, parent, false))
        } else {
            EntryViewHolder(inflater.inflate(R.layout.item_journal_entry_group, parent, false))
        }
    }

    override fun onBindViewHolder(holder: RecyclerView.ViewHolder, position: Int) {
        when (val row = rows[position]) {
            is DateHeaderRow -> (holder as DateHeaderViewHolder).bind(row.label)
            is EntryRow -> (holder as EntryViewHolder).bind(row.entry)
        }
    }

    override fun getItemCount(): Int = rows.size

    private class DateHeaderViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        private val label: TextView = view as TextView
        fun bind(text: String) { label.text = text }
    }

    private inner class EntryViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        private val transactionNumber: TextView = view.findViewById(R.id.textTransactionNumber)
        private val buttonEdit: ImageButton = view.findViewById(R.id.buttonEdit)
        private val buttonDelete: ImageButton = view.findViewById(R.id.buttonDelete)
        private val containerLines: LinearLayout = view.findViewById(R.id.containerLines)

        fun bind(entry: JournalReportEntry) {
            transactionNumber.text = entry.transactionNumber
            val actionsVisibility = if (showActions) View.VISIBLE else View.GONE
            buttonEdit.visibility = actionsVisibility
            buttonDelete.visibility = actionsVisibility
            buttonEdit.setOnClickListener { onEdit(entry) }
            buttonDelete.setOnClickListener { onDelete(entry) }

            containerLines.removeAllViews()
            val inflater = LayoutInflater.from(containerLines.context)
            entry.lines.sortedBy { it.lineOrder }.forEach { line ->
                val lineView = inflater.inflate(R.layout.item_journal_report_line, containerLines, false)
                val nameView = lineView.findViewById<TextView>(R.id.textAccountName)
                val amountView = lineView.findViewById<TextView>(R.id.textAmount)

                if (line.debit > 0) {
                    nameView.text = "${line.referenceNumber} - ${line.accountName}"
                    amountView.text = CurrencyFormatter.format(line.debit)
                } else {
                    // Kredit: indentasi satu tab (spasi) dari debit.
                    nameView.text = "        ${line.referenceNumber} - ${line.accountName}"
                    amountView.text = CurrencyFormatter.format(line.credit)
                }
                containerLines.addView(lineView)
            }
        }
    }
}
