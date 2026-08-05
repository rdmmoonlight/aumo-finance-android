using AumoFinance.Models;

namespace AumoFinance.Services;

public class AccountingService
{
    public Task<AccountingPeriod?> GetCurrentPeriodAsync(Guid currentUserId)
    {
        return Task.FromResult<AccountingPeriod?>(null);
    }

    public Task<List<JournalEntryDisplayModel>> GetGeneralJournalAsync(Guid currentUserId, AccountingPeriod period)
    {
        return Task.FromResult(new List<JournalEntryDisplayModel>());
    }

    public Task<List<LedgerAccountDisplayModel>> GetGeneralLedgerAsync(Guid currentUserId, AccountingPeriod period, bool isTemporary)
    {
        return Task.FromResult(new List<LedgerAccountDisplayModel>());
    }
}
