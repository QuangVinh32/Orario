using FTDNG.Contracts.Events;
using FTDNG.Contracts.Modules;
using FTDNG.Contracts.Queries;
using FTDNG.Host.Wpf.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FTDNG.Host.Wpf.Bootstrap;

/// <summary>
/// Đăng ký các service hạ tầng do Host sở hữu (EventBus, UiRegistry, Shell).
/// Host KHÔNG đăng ký hộ service nghiệp vụ của module — module tự làm qua RegisterServices.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddHostInfrastructure(this IServiceCollection services)
    {
        // Logging chuẩn Microsoft (baseline khuyến nghị dùng abstraction chuẩn).
        services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Information));

        // EventBus tối giản — singleton, đăng ký qua interface ("một việc đã xảy ra").
        services.AddSingleton<IEventBus, EventBus>();

        // QueryBus tối giản — singleton, đăng ký qua interface ("hỏi-lấy dữ liệu ngay").
        services.AddSingleton<IQueryBus, QueryBus>();

        // UiRegistry — singleton. Đăng ký cả concrete (để Shell đọc contribution)
        // lẫn interface (để module đăng ký UI qua IUiRegistry).
        services.AddSingleton<UiRegistry>();
        services.AddSingleton<IUiRegistry>(sp => sp.GetRequiredService<UiRegistry>());

        // ModuleCatalog — dùng khi phát hiện module.
        services.AddSingleton<ModuleCatalog>();

        // Shell (vỏ ứng dụng).
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
