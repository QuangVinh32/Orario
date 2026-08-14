using FTDNG.SharedKernel;

namespace FTDNG.Contracts.Project;

/// <summary>
/// Dữ liệu một section của module ở dạng đã serialize (payload + version).
/// ProjectManager không cần biết kiểu concrete của module.
/// </summary>
public sealed record ProjectSectionData(string SectionKey, int SectionVersion, ReadOnlyMemory<byte> Data);

/// <summary>
/// Ảnh chụp toàn bộ project để Save/Open (Mục 5). Host v1 chỉ khai báo contract;
/// implementation storage thuộc ProjectManager.
/// </summary>
public sealed record ProjectSnapshot(
    ProjectId ProjectId,
    int FormatMajor,
    int FormatMinor,
    IReadOnlyList<ProjectSectionData> Sections);

/// <summary>
/// Mỗi module tự cung cấp/khôi phục section dữ liệu của nó (Mục 5.1).
/// ProjectManager enumerate qua DI mà không reference concrete type của module.
/// </summary>
public interface IProjectSectionProvider
{
    string SectionKey { get; }
    int SectionVersion { get; }
    object Capture();
    void Restore(ProjectSectionData data);
}

/// <summary>
/// Trừu tượng lưu/mở file project. Implementation (ví dụ *.ftmp) thuộc ProjectManager.
/// </summary>
public interface IProjectStorage
{
    Task SaveAsync(ProjectSnapshot snapshot, string path, CancellationToken ct);
    Task<ProjectSnapshot> LoadAsync(string path, CancellationToken ct);
}
