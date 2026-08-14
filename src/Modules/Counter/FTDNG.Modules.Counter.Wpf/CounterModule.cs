using FTDNG.Contracts.Modules;
using FTDNG.Modules.Counter.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FTDNG.Modules.Counter.Wpf;

/// <summary>
/// Hợp đồng của module Counter với Host. Host chỉ biết IFtdngModule này,
/// không biết Counter làm gì bên trong.
/// </summary>
public sealed class CounterModule : IFtdngModule
{
    public string Id => "FTDNG.Modules.Counter";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<CounterViewModel>();
        services.AddTransient<CounterView>();
    }

    public void RegisterUi(IUiRegistry ui)
    {
        // Đặt View ở giữa màn hình. order lớn hơn Sample (100) để xếp phía sau.
        ui.AddView(UiSlot.MainCanvas, typeof(CounterView), order: 200);
    }
}
