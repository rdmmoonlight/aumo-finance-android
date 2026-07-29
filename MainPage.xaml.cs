namespace AumoFinance;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Menangani tombol back fisik Android
    protected override bool OnBackButtonPressed()
    {
        if (WebEngine.CanGoBack)
        {
            WebEngine.GoBack();
            return true; // Cegah aplikasi keluar
        }

        return base.OnBackButtonPressed(); // Keluar aplikasi jika sudah di halaman utama web
    }
}
