using FTDNG.Contracts.Modules;

namespace FTDNG.Host.Wpf.Shell;

/// <summary>Một View mà module đóng góp vào một slot (đã kèm thứ tự).</summary>
public sealed record UiViewContribution(UiSlot Slot, Type ViewType, int Order);

/// <summary>Một command mà module đóng góp vào một location (menu/toolbar).</summary>
public sealed record UiCommandContribution(string Location, UiCommandDescriptor Command);

/// <summary>
/// Sổ đăng ký UI của Host (Mục 4). Chỉ lưu "ai gắn gì vào đâu"; KHÔNG chứa nghiệp vụ.
/// Shell đọc registry này để render. Đăng ký singleton qua DI.
/// </summary>
public sealed class UiRegistry : IUiRegistry
{
    private readonly List<UiViewContribution> _views = new();
    private readonly List<UiCommandContribution> _commands = new();

    public void AddView(UiSlot slot, Type viewType, int order = 0)
    {
        ArgumentNullException.ThrowIfNull(viewType);
        _views.Add(new UiViewContribution(slot, viewType, order));
    }

    public void AddCommand(string location, UiCommandDescriptor command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        ArgumentNullException.ThrowIfNull(command);
        _commands.Add(new UiCommandContribution(location, command));
    }

    /// <summary>View đã đăng ký cho một slot, sắp theo Order rồi thứ tự đăng ký.</summary>
    public IReadOnlyList<UiViewContribution> GetViews(UiSlot slot) =>
        _views.Where(v => v.Slot == slot)
              .OrderBy(v => v.Order)
              .ToList();

    /// <summary>Command đã đăng ký cho một location, sắp theo Order.</summary>
    public IReadOnlyList<UiCommandContribution> GetCommands(string location) =>
        _commands.Where(c => string.Equals(c.Location, location, StringComparison.OrdinalIgnoreCase))
                 .OrderBy(c => c.Command.Order)
                 .ToList();
}
