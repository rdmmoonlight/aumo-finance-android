package com.aumofinance.app.dashboard

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R

class CashAccountAdapter(private var items: List<CashAccountEntry>) :
    RecyclerView.Adapter<CashAccountAdapter.ViewHolder>() {

    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val name: TextView = view.findViewById(R.id.textAccountName)
        val balance: TextView = view.findViewById(R.id.textAccountBalance)
    }

    fun submitList(newItems: List<CashAccountEntry>) {
        items = newItems
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_cash_account, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val item = items[position]
        holder.name.text = item.accountName
        holder.balance.text = CurrencyFormatter.format(item.balance)
    }

    override fun getItemCount(): Int = items.size
}
