package com.aumofinance.app.reports.trialbalance

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R

class TrialBalanceAdapter(private var rows: List<TrialBalanceRow>) :
    RecyclerView.Adapter<TrialBalanceAdapter.ViewHolder>() {

    fun submitList(newRows: List<TrialBalanceRow>) {
        rows = newRows
        notifyDataSetChanged()
    }

    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val name: TextView = view.findViewById(R.id.textAccountName)
        val debit: TextView = view.findViewById(R.id.textDebit)
        val credit: TextView = view.findViewById(R.id.textCredit)
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_trial_balance_row, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val row = rows[position]
        holder.name.text = "${row.referenceNumber} - ${row.accountName}"
        holder.debit.text = if (row.debit > 0) CurrencyFormatter.format(row.debit) else ""
        holder.credit.text = if (row.credit > 0) CurrencyFormatter.format(row.credit) else ""
    }

    override fun getItemCount(): Int = rows.size
}
