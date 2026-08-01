namespace AumoMobile.Views;

public partial class TopBarView : ContentView
{
    public TopBarView()
    {
        InitializeComponent();
        LoadAppVersion();
    }

    private void LoadAppVersion()
    {
        // Membaca versi otomatis dari AppInfo (Package Version Android/iOS)
        var version = AppInfo.Current.VersionString; // Contoh: "1.0.1"
        var build = AppInfo.Current.BuildString;     // Contoh: "42" (github.run_number)
        
        VersionLabel.Text = $"v{version} ({build})";
    }
}
