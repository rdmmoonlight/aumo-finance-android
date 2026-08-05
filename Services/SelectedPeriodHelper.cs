using AumoFinance;
using AumoFinance.Models;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Services;

public static class SelectedPeriodHelper
{
    public static async Task<Period?> GetSelectedPeriodAsync(AppDbContext dbContext, Guid userId)
    {
        return await dbContext.SelectedPeriods
            .Include(s => s.Period)
            .Where(s => s.UserId == userId)
            .Select(s => s.Period)
            .FirstOrDefaultAsync();
    }

    public static async Task SelectPeriodAsync(AppDbContext dbContext, Guid userId, Guid periodId)
    {
        var selected = await dbContext.SelectedPeriods.FirstOrDefaultAsync(s => s.UserId == userId);
        if (selected == null)
        {
            dbContext.SelectedPeriods.Add(new SelectedPeriod { UserId = userId, PeriodId = periodId });
        }
        else
        {
            selected.PeriodId = periodId;
        }

        await dbContext.SaveChangesAsync();
    }

    public static async Task ClearSelectionAsync(AppDbContext dbContext, Guid userId)
    {
        var selections = await dbContext.SelectedPeriods.Where(s => s.UserId == userId).ToListAsync();
        dbContext.SelectedPeriods.RemoveRange(selections);
        await dbContext.SaveChangesAsync();
    }
}
