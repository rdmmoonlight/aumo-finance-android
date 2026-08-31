package com.aumofinance.app.periods

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.R

class PeriodsAdapter(
    private var items: List<Period>,
    private var selectedPeriodId: Int?,
    private val onSelect: (Period) -> Unit,
    private val onClose: (Period) -> Unit
) : RecyclerView.Adapter<PeriodsAdapter.ViewHolder>() {

    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val name: TextView = view.findViewById(R.id.textPeriodName)
        val status: TextView = view.findViewById(R.id.textPeriodStatus)
        val range: TextView = view.findViewById(R.id.textPeriodRange)
        val selectButton: Button = view.findViewById(R.id.buttonSelect)
        val closeButton: Button = view.findViewById(R.id.buttonClose)
    }

    fun submitList(newItems: List<Period>, newSelectedId: Int?) {
        items = newItems
        selectedPeriodId = newSelectedId
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_period, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val period = items[position]
        holder.name.text = period.periodName
        holder.range.text = "${period.startDate} - ${period.endDate}"

        holder.status.text = when {
            period.isClosed -> "Ditutup"
            period.id == selectedPeriodId -> "Sedang Dilihat"
            else -> "Belum dipilih"
        }

        holder.closeButton.visibility = if (period.isClosed) View.GONE else View.VISIBLE
        holder.selectButton.isEnabled = period.id != selectedPeriodId

        holder.selectButton.setOnClickListener { onSelect(period) }
        holder.closeButton.setOnClickListener { onClose(period) }
    }

    override fun getItemCount(): Int = items.size
}
