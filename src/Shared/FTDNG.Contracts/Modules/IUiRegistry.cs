namespace FTDNG.Contracts.Modules;

/// <summary>
/// Sổ đăng ký phần UI mà module đóng góp cho Host (Mục 4.x tài liệu).
/// Module khai báo "View/Command nằm ở slot/location nào"; Host chịu trách nhiệm render.
/// RegisterUi KHÔNG đưa nghiệp vụ vào Host — chỉ đăng ký contribution theo slot.
/// </summary>
public interface IUiRegistry
{
    /// <summary>
    /// Đăng ký một View (Type của control/UserControl) vào một <see cref="UiSlot"/>.
    /// Host sẽ resolve <paramref name="viewType"/> qua DI khi render.
    /// </summary>
    /// <param name="slot">Vị trí logic trên Shell.</param>
    /// <param name="viewType">Kiểu View; Host resolve và gắn vào slot. Truyền Type để giữ Contracts UI-free.</param>
    /// <param name="order">Thứ tự trong slot (nhỏ hơn hiển thị trước).</param>
    void AddView(UiSlot slot, Type viewType, int order = 0);

    /// <summary>
    /// Đăng ký một command vào một location logic (ví dụ "menu.edit", "toolbar.main").
    /// </summary>
    void AddCommand(string location, UiCommandDescriptor command);
}
