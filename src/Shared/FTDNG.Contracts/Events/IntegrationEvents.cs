using FTDNG.SharedKernel;

namespace FTDNG.Contracts.Events;

// Event xuyên module (Mục 4.1). Chỉ mang ID/DTO UI-neutral, KHÔNG mang WPF View/Control.

public sealed record ProjectOpenedEvent(ProjectId ProjectId);

public sealed record CalendarChangedEvent(CalendarId CalendarId);

public sealed record WbsChangedEvent(ProjectId ProjectId);

public sealed record BarCreatedEvent(BarId BarId, TaskRowId RowId);

public sealed record DependencyChangedEvent(DependencyId DependencyId);

/// <summary>Event demo chiều ngược: Notifier phát, module khác (vd Sample) lắng nghe.</summary>
public sealed record NotifierPingEvent(string Text);
