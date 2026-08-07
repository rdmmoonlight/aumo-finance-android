using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace AumoFinance.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class ReminderBroadcastReceiver : BroadcastReceiver
{
    public const string ActionShowReminder = "com.aumofinance.ACTION_SHOW_REMINDER";
    private const int NotificationId = 1001;

    public override void OnReceive(Context? context, Intent? intent)
    {
        // Guard clause untuk mencegah dereference jika context/intent null
        if (context == null || intent == null)
        {
            return;
        }

        if (intent.Action != ActionShowReminder)
        {
            return;
        }

        NotificationService.EnsureChannel();

        // Menggunakan ContextCompat agar aman dari masalah null intent launcher
        var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName);
        
        PendingIntent? pendingIntent = null;
        if (launchIntent != null)
        {
            pendingIntent = PendingIntent.GetActivity(
                context,
                0,
                launchIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        }

        // Baris 59: Penggunaan safe-navigation ?. dan fallback default string
        var builder = new NotificationCompat.Builder(context, NotificationService.ChannelId)
            .SetSmallResource(global::Android.Resource.Drawable.IcDialogInfo) // Sesuaikan icon jika ada resource lokal (contoh: Resource.Drawable.ic_stat_name)
            .SetContentTitle("AumoFinance")
            .SetContentText("Jangan lupa catat transaksi keuanganmu hari ini!")
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityDefault);

        if (pendingIntent != null)
        {
            builder.SetContentIntent(pendingIntent);
        }

        // Baris 71: Menggunakan safe cast dan null check pada GetSystemService & Notify
        var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
        notificationManager?.Notify(NotificationId, builder.Build());
    }
}
