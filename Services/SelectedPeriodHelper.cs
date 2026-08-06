using AumoFinance;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Services;

public static class SelectedPeriodHelper
{
    public static async Task<Models.Period?> GetSelectedPeriodAsync(AppDbContext dbContext, Guid userId)
    {
        return await dbContext.Periods
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsSelected);
    }

    public static async Task SelectPeriodAsync(AppDbContext dbContext, Guid userId, int periodId)
    {
        var periods = await dbContext.Periods
            .Where(p => p.UserId == userId)
            .ToListAsync();

        foreach (var period in periods)
        {
            period.IsSelected = period.Id == periodId;
        }

        await dbContext.SaveChangesAsync();
    }

    public static async Task ClearSelectionAsync(AppDbContext dbContext, Guid userId)
    {
        var selected = await dbContext.Periods
            .Where(p => p.UserId == userId && p.IsSelected)
            .ToListAsync();

        foreach (var period in selected)
        {
            period.IsSelected = false;
        }

        await dbContext.SaveChangesAsync();
    }
}
