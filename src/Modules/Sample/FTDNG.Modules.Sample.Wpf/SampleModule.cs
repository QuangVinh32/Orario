using FTDNG.Contracts.Modules;
using FTDNG.Modules.Sample.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FTDNG.Modules.Sample.Wpf;

/// <summary>
/// Module demo hiện thực <see cref="IFtdngModule"/>. Chứng minh: Host phát hiện qua interface,
/// module tự đăng ký DI + tự gắn View/Command vào slot. Host không biết logic bên trong.
/// </summary>
public sealed class SampleModule : IFtdngModule
{
    public string Id => "FTDNG.Modules.Sample";

    public void RegisterServices(IServiceCollection services)
    {
        // Module tự đăng ký ViewModel/service/View của chính mình.
        services.AddSingleton<SampleViewModel>();
        services.AddTransient<SampleLeftView>();
        services.AddTransient<SampleCanvasView>();
    }

    public void RegisterUi(IUiRegistry ui)
    {
        // Tự khai báo View nằm ở slot nào — Host chỉ render.
        ui.AddView(UiSlot.LeftPanel, typeof(SampleLeftView), order: 100);
        ui.AddView(UiSlot.MainCanvas, typeof(SampleCanvasView), order: 100);

        // Đóng góp command vào toolbar và menu Edit của Host.
        ui.AddCommand("toolbar.main", new UiCommandDescriptor
        {
            Id = "sample.hello",
            Title = "Sample: Chào",
            Order = 10,
            Execute = () => System.Windows.MessageBox.Show(
                "Xin chào từ Sample module hẹ hẹ hẹ!", "Sample", System.Windows.MessageBoxButton.OK)
        });

        ui.AddCommand("menu.edit", new UiCommandDescriptor
        {
            Id = "sample.menu",
            Title = "Sample Command...",
            Order = 10,
            InputGestureText = "Ctrl+Shift+S",
            Execute = () => System.Windows.MessageBox.Show(
                "Command này do Sample module đăng ký vào menu Edit.", "Sample",
                System.Windows.MessageBoxButton.OK)
        });
    }
}
