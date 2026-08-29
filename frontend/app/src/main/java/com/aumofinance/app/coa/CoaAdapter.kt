package com.aumofinance.app.coa

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.R

class CoaAdapter(
    private var items: List<Account>,
    private val onClick: (Account) -> Unit
) : RecyclerView.Adapter<CoaAdapter.ViewHolder>() {

    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val title: TextView = view.findViewById(R.id.textAccountTitle)
        val type: TextView = view.findViewById(R.id.textAccountType)
        val balance: TextView = view.findViewById(R.id.textAccountBalance)
        val badge: TextView = view.findViewById(R.id.textAccountBadge)
    }

    fun submitList(newItems: List<Account>) {
        items = newItems
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_account, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val account = items[position]
        holder.title.text = "${account.referenceNumber} - ${account.accountName}"
        holder.type.text = account.type
        holder.balance.text = CurrencyFormatter.format(account.balance)
        holder.badge.visibility = if (account.isActive) View.GONE else View.VISIBLE
        holder.itemView.setOnClickListener { onClick(account) }
    }

    override fun getItemCount(): Int = items.size
}
