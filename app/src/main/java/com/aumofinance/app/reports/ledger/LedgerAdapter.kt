package com.aumofinance.app.reports.ledger

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.LinearLayout
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R
import java.text.SimpleDateFormat
import java.util.Locale

// T-account style: satu blok per akun, berisi daftar mutasi (tanggal,
// deskripsi, debit ATAU kredit, saldo berjalan) dan saldo akhir di footer.
class LedgerAdapter(private var accounts: List<LedgerAccount>) :
    RecyclerView.Adapter<LedgerAdapter.ViewHolder>() {

    private val inputDateFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US)
    private val displayDateFormat = SimpleDateFormat("dd/MM", Locale("in", "ID"))

    fun submitList(newAccounts: List<LedgerAccount>) {
        accounts = newAccounts
        notifyDataSetChanged()
    }

    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val title: TextView = view.findViewById(R.id.textAccountTitle)
        val containerLines: LinearLayout = view.findViewById(R.id.containerLines)
        val endingBalance: TextView = view.findViewById(R.id.textEndingBalance)
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_ledger_account, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val account = accounts[position]
        holder.title.text = "${account.referenceNumber} - ${account.accountName}"
        holder.endingBalance.text = CurrencyFormatter.format(account.endingBalance)

        holder.containerLines.removeAllViews()
        val inflater = LayoutInflater.from(holder.containerLines.context)
        account.lines.forEach { line ->
            val lineView = inflater.inflate(R.layout.item_ledger_line, holder.containerLines, false)
            lineView.findViewById<TextView>(R.id.textDate).text = formatDate(line.entryDate)
            lineView.findViewById<TextView>(R.id.textDescription).text = line.description ?: ""
            val amount = if (line.debit > 0) line.debit else -line.credit
            lineView.findViewById<TextView>(R.id.textDebitCredit).text = CurrencyFormatter.format(amount)
            lineView.findViewById<TextView>(R.id.textRunningBalance).text = CurrencyFormatter.format(line.runningBalance)
            holder.containerLines.addView(lineView)
        }
    }

    override fun getItemCount(): Int = accounts.size

    private fun formatDate(iso: String): String = try {
        displayDateFormat.format(inputDateFormat.parse(iso)!!)
    } catch (e: Exception) {
        iso
    }
}
