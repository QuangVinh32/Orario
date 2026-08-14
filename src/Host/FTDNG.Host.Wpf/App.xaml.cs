using System.Windows;
using FTDNG.Contracts.Modules;
using FTDNG.Host.Wpf.Bootstrap;
using FTDNG.Host.Wpf.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FTDNG.Host.Wpf;

/// <summary>
/// Composition Root của ứng dụng (Mục 4). App/Shell dựng vỏ và ghép toàn bộ dependency;
/// Host biết "có module" nhưng KHÔNG biết nghiệp vụ bên trong module.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _provider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Logger tạm cho giai đoạn discovery (trước khi có ServiceProvider).
        using var bootstrapLoggerFactory = LoggerFactory.Create(b => b.AddDebug().SetMinimumLevel(LogLevel.Information));
        var log = bootstrapLoggerFactory.CreateLogger<App>();

        // (1) Khởi tạo IServiceCollection + hạ tầng Host (EventBus, UiRegistry, Shell).
        var services = new ServiceCollection();
        services.AddHostInfrastructure();

        // (2) Phát hiện module: Host chỉ biết interface IFtdngModule.
        var catalog = new ModuleCatalog(bootstrapLoggerFactory.CreateLogger<ModuleCatalog>());
        IReadOnlyList<IFtdngModule> modules = catalog.DiscoverModules();

        // (3) Mỗi module tự đăng ký service/ViewModel/implementation của chính nó.
        foreach (var module in modules)
        {
            try
            {
                module.RegisterServices(services);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Module {ModuleId} lỗi khi RegisterServices.", module.Id);
            }
        }

        // (4) Sau khi mọi module đã đăng ký, mới build ServiceProvider để dependency đầy đủ.
        _provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = false // module tự chịu trách nhiệm đăng ký đủ dependency của mình
        });

        // (5) Mỗi module đăng ký View/Command vào UiRegistry (dependency đã sẵn sàng).
        var uiRegistry = _provider.GetRequiredService<UiRegistry>();
        foreach (var module in modules)
        {
            try
            {
                module.RegisterUi(uiRegistry);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Module {ModuleId} lỗi khi RegisterUi.", module.Id);
            }
        }

        // (6) Host render: MainWindow đọc UiRegistry, resolve View qua DI rồi ghép vào slot.
        var main = _provider.GetRequiredService<MainWindow>();
        MainWindow = main;
        main.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _provider?.Dispose();
        base.OnExit(e);
    }
}
