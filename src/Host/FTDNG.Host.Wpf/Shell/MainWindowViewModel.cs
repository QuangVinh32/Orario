using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using FTDNG.Contracts.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FTDNG.Host.Wpf.Shell;

/// <summary>Phần tử command hiển thị trên toolbar/menu (Title + ICommand đã bọc).</summary>
public sealed record ShellCommandItem(string Title, System.Windows.Input.ICommand Command, string? InputGestureText);

/// <summary>
/// ViewModel của Shell. Đọc <see cref="UiRegistry"/>, resolve View qua DI rồi ghép vào slot.
/// KHÔNG chứa nghiệp vụ Calendar/WBS/Gantt — chỉ là khung hiển thị (Mục 4.2).
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IServiceProvider _services;
    private readonly UiRegistry _registry;
    private readonly ILogger<MainWindowViewModel>? _logger;

    public MainWindowViewModel(
        IServiceProvider services,
        UiRegistry registry,
        ILogger<MainWindowViewModel>? logger = null)
    {
        _services = services;
        _registry = registry;
        _logger = logger;
    }

    public string Title { get; } = "FTDNG Kotei — Host (Beta 0.0.0)";

    private string _statusText = "Sẵn sàng.";
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
    }

    // Mỗi slot là một danh sách phần tử UI đã resolve (đặt trực tiếp FrameworkElement).
    public ObservableCollection<UIElement> ToolbarViews { get; } = new();
    public ObservableCollection<UIElement> LeftPanelViews { get; } = new();
    public ObservableCollection<UIElement> MainCanvasViews { get; } = new();
    public ObservableCollection<UIElement> BottomPanelViews { get; } = new();
    public ObservableCollection<UIElement> StatusBarViews { get; } = new();

    // Command đóng góp cho toolbar/menu.
    public ObservableCollection<ShellCommandItem> ToolbarCommands { get; } = new();
    public ObservableCollection<ShellCommandItem> MenuCommands { get; } = new();

    /// <summary>
    /// Ghép toàn bộ contribution vào slot. Gọi trên UI thread sau khi provider đã build.
    /// </summary>
    public void Compose()
    {
        FillViews(UiSlot.Toolbar, ToolbarViews);
        FillViews(UiSlot.LeftPanel, LeftPanelViews);
        FillViews(UiSlot.MainCanvas, MainCanvasViews);
        FillViews(UiSlot.BottomPanel, BottomPanelViews);
        FillViews(UiSlot.StatusBar, StatusBarViews);

        FillCommands("toolbar.main", ToolbarCommands);
        FillCommands("menu.edit", MenuCommands);

        var viewCount = ToolbarViews.Count + LeftPanelViews.Count + MainCanvasViews.Count
                        + BottomPanelViews.Count + StatusBarViews.Count;
        StatusText = $"Đã ghép {viewCount} view, {ToolbarCommands.Count + MenuCommands.Count} command.";
    }

    private void FillViews(UiSlot slot, ObservableCollection<UIElement> target)
    {
        target.Clear();
        foreach (var contribution in _registry.GetViews(slot))
        {
            // Host resolve View qua DI — KHÔNG new bằng tay (Mục 4.2 "điều Host KHÔNG được làm").
            var resolved = _services.GetService(contribution.ViewType);
            if (resolved is UIElement element)
            {
                target.Add(element);
            }
            else
            {
                _logger?.LogWarning(
                    "View {ViewType} ở slot {Slot} không resolve được hoặc không phải UIElement.",
                    contribution.ViewType.FullName, slot);
            }
        }
    }

    private void FillCommands(string location, ObservableCollection<ShellCommandItem> target)
    {
        target.Clear();
        foreach (var c in _registry.GetCommands(location))
        {
            var cmd = new RelayCommand(c.Command.Execute, c.Command.CanExecute);
            target.Add(new ShellCommandItem(c.Command.Title, cmd, c.Command.InputGestureText));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
