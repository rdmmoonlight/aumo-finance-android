using System;
using System.Threading.Tasks;
using Plugin.LocalNotification;

namespace AumoFinance.Services;

public class NotificationService
{
    public async Task<bool> RequestPermissionAsync()
    {
        // Untuk Android 12 ke bawah (termasuk Android 9), ini akan bernilai true otomatis
        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            return await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
        return true;
    }

    public async Task ScheduleDailyReminderAsync(int hour = 20, int minute = 0)
    {
        var now = DateTime.Now;
        var scheduledTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

        if (now > scheduledTime)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        var request = new NotificationRequest
        {
            NotificationId = 1001,
            Title = "Sudah Catat Keuangan Hari Ini? 💎",
            Description = "Jangan lupa rapikan dan catat transaksi pengeluaran/pemasukan AumoFinance kamu ya!",
            BadgeNumber = 1,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = scheduledTime,
                RepeatType = NotificationRepeat.Daily
            }
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public void CancelDailyReminder()
    {
        LocalNotificationCenter.Current.Cancel(1001);
    }
}
