using Microsoft.Extensions.DependencyInjection;

namespace FTDNG.Contracts.Modules;

/// <summary>
/// Hợp đồng mà mọi module phải hiện thực để Host phát hiện và nạp (Mục 4 tài liệu).
/// Host chỉ biết interface này, KHÔNG biết logic cụ thể bên trong module.
/// </summary>
public interface IFtdngModule
{
    /// <summary>Định danh module ổn định (ví dụ "FTDNG.GanttChart").</summary>
    string Id { get; }

    /// <summary>
    /// Module tự đăng ký ViewModel/service/implementation của chính nó vào DI.
    /// Được gọi TRƯỚC khi Host build ServiceProvider.
    /// </summary>
    void RegisterServices(IServiceCollection services);

    /// <summary>
    /// Module khai báo View/Command của mình vào <see cref="IUiRegistry"/>.
    /// Được gọi SAU khi ServiceProvider đã build (dependency đầy đủ).
    /// </summary>
    void RegisterUi(IUiRegistry ui);
}
