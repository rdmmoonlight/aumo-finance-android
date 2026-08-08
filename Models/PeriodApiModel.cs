using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AumoFinance.Models;

public class PeriodApiModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("periodName")]
    public string PeriodName { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("isClosed")]
    public bool IsClosed { get; set; }

    [JsonPropertyName("isSelected")]
    public bool IsSelected { get; set; }

    public string DateRangeDisplay => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
    public bool CanSelect => !IsSelected;
    public bool CanClose => !IsClosed;
}

// Wrapper respons GET /api/mobile/periods: { success, selectedPeriodId, periods: [...] }
public class PeriodsEnvelopeModel
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("selectedPeriodId")]
    public int? SelectedPeriodId { get; set; }

    [JsonPropertyName("periods")]
    public List<PeriodApiModel> Periods { get; set; } = new();
}
