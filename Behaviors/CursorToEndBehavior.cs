using Microsoft.Maui.Controls;

namespace AumoFinance.Behaviors;

/// <summary>
/// Menempatkan kursor selalu di akhir teks setiap kali Text berubah.
///
/// Dipakai pada Entry yang teksnya diformat ulang otomatis saat mengetik
/// (mis. pemisah ribuan Debit/Kredit di JournalLineViewModel). Tanpa ini:
/// saat panjang teks berubah karena titik ribuan ditambahkan (mis.
/// "2375" -> "2.375"), Android mempertahankan index kursor LAMA (posisi
/// sebelum reformat), sehingga digit berikutnya masuk di posisi yang
/// salah — inilah sebab mengetik "237500" bisa menghasilkan "237.005"
/// alih-alih "237.500". Dengan behavior ini kursor selalu didorong ke
/// akhir, jadi setiap digit baru selalu ditambahkan di ujung — sesuai
/// pola pengetikan normal untuk input nominal uang.
/// </summary>
public class CursorToEndBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry entry)
    {
        base.OnAttachedTo(entry);
        entry.TextChanged += OnTextChanged;
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(entry);
    }

    private static void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry) return;

        int length = entry.Text?.Length ?? 0;
        if (entry.CursorPosition != length)
        {
            entry.CursorPosition = length;
        }
    }
}
