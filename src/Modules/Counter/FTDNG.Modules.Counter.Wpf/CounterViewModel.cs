using System.ComponentModel;
using System.Windows;
namespace FTDNG.Modules.Counter.Wpf;

/// <summary>
/// ViewModel của Counter: giữ state (Count) + nghiệp vụ (Increase/Reset).
/// Thuần logic, không dính Host, không dùng WPF control → dễ unit test.
/// </summary>
public sealed class CounterViewModel : INotifyPropertyChanged
{
    private int _count;
    public int Count
    {
        get => _count;
        private set { _count = value; OnPropertyChanged(nameof(Count)); }
    }

    public void Increase()
    {
        Count++;
        if (Count == 10)
            System.Windows.MessageBox.Show(
                "Đếm tới 10 rồi!",
                "Counter",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
    }

    public void Decrease()
    {
        Count--;
        if (Count == -10)
        {
            MessageBox.Show(
                "Đếm xuống -10 rồi!",
                "Counter",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
    }

    public void Reset() => Count = 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
