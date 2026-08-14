using System.IO;
using System.Reflection;
using FTDNG.Contracts.Modules;
using Microsoft.Extensions.Logging;

namespace FTDNG.Host.Wpf.Bootstrap;

/// <summary>
/// Phát hiện các <see cref="IFtdngModule"/> có trong bản cài đặt (Mục 4).
/// Host chỉ biết interface IFtdngModule; ModuleCatalog quét assembly và khởi tạo module,
/// KHÔNG hiểu logic nghiệp vụ bên trong.
/// </summary>
public sealed class ModuleCatalog
{
    private readonly ILogger<ModuleCatalog>? _logger;

    /// <summary>Thư mục con chứa DLL module được copy cạnh Host lúc build.</summary>
    public const string ModulesFolderName = "Modules";

    public ModuleCatalog(ILogger<ModuleCatalog>? logger = null) => _logger = logger;

    /// <summary>
    /// Quét assembly và trả về danh sách instance module đã khởi tạo.
    /// Nguồn quét: (1) assembly đã nạp trong AppDomain, (2) DLL trong thư mục "Modules/".
    /// </summary>
    public IReadOnlyList<IFtdngModule> DiscoverModules()
    {
        var moduleTypes = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var asm in CollectCandidateAssemblies())
        {
            foreach (var type in SafeGetModuleTypes(asm))
            {
                // Dedupe theo full name để một type không bị nạp 2 lần từ 2 nguồn.
                moduleTypes[type.FullName ?? type.Name] = type;
            }
        }

        var modules = new List<IFtdngModule>();
        foreach (var type in moduleTypes.Values)
        {
            try
            {
                if (Activator.CreateInstance(type) is IFtdngModule module)
                {
                    modules.Add(module);
                    _logger?.LogInformation("Phát hiện module {ModuleId} ({Type}).", module.Id, type.FullName);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Không khởi tạo được module {Type}.", type.FullName);
            }
        }

        _logger?.LogInformation("Tổng số module phát hiện: {Count}.", modules.Count);
        return modules;
    }

    private IEnumerable<Assembly> CollectCandidateAssemblies()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // (1) Các assembly đã nạp (bao gồm chính Host và những gì đã tham chiếu).
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!asm.IsDynamic && seen.Add(asm.FullName ?? asm.GetName().Name ?? string.Empty))
                yield return asm;
        }

        // (2) DLL trong thư mục "Modules/" cạnh Host (module nạp động, không compile-time coupling).
        var modulesDir = Path.Combine(AppContext.BaseDirectory, ModulesFolderName);
        if (!Directory.Exists(modulesDir))
            yield break;

        foreach (var dll in Directory.EnumerateFiles(modulesDir, "*.dll", SearchOption.TopDirectoryOnly))
        {
            Assembly? asm = null;
            try
            {
                asm = Assembly.LoadFrom(dll);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Bỏ qua DLL không nạp được: {Dll}.", dll);
            }

            if (asm is not null && seen.Add(asm.FullName ?? asm.GetName().Name ?? dll))
                yield return asm;
        }
    }

    private IEnumerable<Type> SafeGetModuleTypes(Assembly asm)
    {
        Type[] types;
        try
        {
            types = asm.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Một số type lỗi không nên chặn cả assembly.
            types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Không đọc được type từ {Assembly}.", asm.FullName);
            yield break;
        }

        foreach (var t in types)
        {
            if (t is { IsAbstract: false, IsInterface: false }
                && typeof(IFtdngModule).IsAssignableFrom(t)
                && t.GetConstructor(Type.EmptyTypes) is not null)
            {
                yield return t;
            }
        }
    }
}
