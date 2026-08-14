using FTDNG.Contracts.Modules;
using FTDNG.Modules.Notifier.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FTDNG.Modules.Notifier.Wpf;

/// <summary>
/// Module thứ 2, độc lập hoàn toàn với Sample. Nhiệm vụ: lắng nghe các event
/// (vd ProjectOpenedEvent do Sample phát) và hiển thị log ở BottomPanel.
/// KHÔNG hề reference tới FTDNG.Modules.Sample — chỉ biết kiểu event chung trong Contracts.
/// </summary>
public sealed class NotifierModule : IFtdngModule
{
    public string Id => "FTDNG.Modules.Notifier";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<NotifierViewModel>();
        services.AddTransient<NotifierPanelView>();
    }

    public void RegisterUi(IUiRegistry ui)
    {
        // Gắn 1 view vào BottomPanel để hiện log sự kiện nhận được.
        ui.AddView(UiSlot.BottomPanel, typeof(NotifierPanelView), order: 100);
    }
}
