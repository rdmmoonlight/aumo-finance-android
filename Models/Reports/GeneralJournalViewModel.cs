using System;
using System.Collections.Generic;
using System.Globalization;

namespace AumoFinance.Models.Reports;

public class GeneralJournalEntryViewModel
{
    public int Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string JournalType { get; set; } = "General";
    public string ReferenceNumber { get; set; } = string.Empty;
    public List<GeneralJournalLineViewModel> Lines { get; set; } = new();
    public CultureInfo UsdCulture { get; set; } = new("en-US");

    public string FormattedDate => EntryDate.ToString("MMM dd, yyyy");
}

public class GeneralJournalLineViewModel
{
    public int AccountReferenceNumber { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? LineDescription { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public CultureInfo UsdCulture { get; set; } = new("en-US");

    public string FormattedDebit => Debit > 0 ? Debit.ToString("C2", UsdCulture) : "-";
    public string FormattedCredit => Credit > 0 ? Credit.ToString("C2", UsdCulture) : "-";
}
