namespace FTDNG.Contracts.Modules;

/// <summary>
/// Các "chỗ trống" (slot) mà Host cung cấp để module gắn UI vào.
/// Host chỉ biết slot; không biết nghiệp vụ bên trong module (xem Mục 4 tài liệu).
/// </summary>
public enum UiSlot
{
    Toolbar,
    LeftPanel,
    MainCanvas,
    BottomPanel,
    StatusBar
}
