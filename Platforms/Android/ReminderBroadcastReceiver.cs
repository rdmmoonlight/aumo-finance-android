using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AumoFinance.Services; // Menyesuaikan CS0103 agar NotificationService terdeteksi

namespace AumoFinance.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class ReminderBroadcastReceiver : BroadcastReceiver
{
    public const string ActionShowReminder = "com.aumofinance.ACTION_SHOW_REMINDER";
    private const int NotificationId = 1001;

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null)
        {
            return;
        }

        if (intent.Action != ActionShowReminder)
        {
            return;
        }

        NotificationService.EnsureChannel();

        // Mengatasi CS8604 dengan memastikan context.PackageName tidak null
        string packageName = context.PackageName ?? string.Empty;
        
        PendingIntent? pendingIntent = null;
        if (!string.IsNullOrEmpty(packageName))
        {
            var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(packageName);
            if (launchIntent != null)
            {
                pendingIntent = PendingIntent.GetActivity(
                    context,
                    0,
                    launchIntent,
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            }
        }

        var builder = new NotificationCompat.Builder(context, NotificationService.ChannelId)
            .SetSmallResource(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentTitle("AumoFinance")
            .SetContentText("Jangan lupa catat transaksi keuanganmu hari ini!")
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityDefault);

        if (pendingIntent != null)
        {
            builder.SetContentIntent(pendingIntent);
        }

        var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
        notificationManager?.Notify(NotificationId, builder.Build());
    }
}
