# FTDNG Kotei — Host v1

Bản hiện thực **Host v1** theo Mục 3–4 của *TÀI_LIỆU_DỰ_ÁN_FTDNG_Kotei_Beta_0.0.0_Rev05*:
**App/Shell + Composition Root + DI + ModuleCatalog + UiRegistry** (C# + WPF + MVVM, Microsoft DI, .NET 8).

## Phạm vi Host v1

Host chỉ làm **Composition Root**: biết "có module" nhưng **không biết nghiệp vụ bên trong module**.
Host cung cấp *vỏ ứng dụng* và các *slot* để module tự gắn UI; Host không viết hộ ViewModel/logic của module.

## Cấu trúc solution

```
FTDNG.sln
global.json                      # ghim SDK .NET 8
src/
├─ Host/FTDNG.Host.Wpf/          # WinExe, WPF
│  ├─ App.xaml(.cs)              # Composition Root — điều phối 6 bước bootstrap
│  ├─ Bootstrap/
│  │  ├─ ModuleCatalog.cs        # phát hiện IFtdngModule (assembly + thư mục Modules/)
│  │  ├─ ServiceRegistration.cs  # đăng ký hạ tầng Host vào DI
│  │  ├─ EventBus.cs             # EventBus tối giản, in-process, thread-safe ("việc đã xảy ra")
│  │  └─ QueryBus.cs             # QueryBus tối giản — "hỏi-lấy dữ liệu ngay", mỗi query 1 nguồn
│  └─ Shell/
│     ├─ MainWindow.xaml(.cs)    # 5 slot: Toolbar/LeftPanel/MainCanvas/BottomPanel/StatusBar
│     ├─ MainWindowViewModel.cs  # đọc UiRegistry, resolve View qua DI, ghép slot
│     ├─ UiRegistry.cs           # IUiRegistry — sổ đăng ký UI của module
│     └─ RelayCommand.cs
├─ Shared/
│  ├─ FTDNG.SharedKernel/        # primitive UI-free: Ids, TimeRange, Result/Error
│  └─ FTDNG.Contracts/           # IFtdngModule, IUiRegistry, UiSlot, IEventBus, IQueryBus,
│                                #   integration events, ProjectSession contracts
└─ Modules/
   └─ Sample/FTDNG.Modules.Sample.Wpf/   # module DEMO chứng minh pipeline
```

## Luồng bootstrap (Mục 4.2) — trong `App.OnStartup`

1. Khởi tạo `IServiceCollection` + hạ tầng Host (EventBus, UiRegistry, Shell).
2. `ModuleCatalog` phát hiện các `IFtdngModule`.
3. Từng module `RegisterServices()` vào DI.
4. **Sau đó** mới `BuildServiceProvider()` (dependency đầy đủ).
5. Từng module `RegisterUi()` vào `UiRegistry`.
6. Host render: `MainWindow` đọc `UiRegistry`, resolve View qua DI, ghép vào slot.

## Cơ chế nạp module (không coupling compile-time)

- Host **không** reference output assembly của module (`ReferenceOutputAssembly=false`).
- Khi build, DLL module được copy vào thư mục **`Modules/`** cạnh Host.
- `ModuleCatalog` quét assembly đã nạp **và** thư mục `Modules/`, tìm type hiện thực `IFtdngModule`,
  khởi tạo qua reflection. Nếu một module không có mặt, slot tương ứng trống mà Host vẫn chạy.

> Thêm module mới: tạo project `FTDNG.Modules.<Tên>.Wpf` hiện thực `IFtdngModule`, rồi thêm
> một `ProjectReference` (ReferenceOutputAssembly=false) + dòng copy trong `FTDNG.Host.Wpf.csproj`.

## Build & Run

```powershell
dotnet build FTDNG.sln -c Debug
dotnet run --project src/Host/FTDNG.Host.Wpf/FTDNG.Host.Wpf.csproj
```

Khi chạy, module demo tự gắn: một view vào **LeftPanel**, một view vào **MainCanvas**
(có nút phát `ProjectOpenedEvent` qua EventBus), một lệnh vào **Toolbar** và một lệnh vào menu **Edit**.

## Ranh giới kiến trúc đang được giữ

- `SharedKernel` và `Contracts` **UI-free** (không reference `System.Windows`/WPF).
- Host **không** `new` ViewModel của module, **không** chứa `CreateBar()/SaveCalendar()/...`,
  **không** reference concrete service của module.
- EventBus dùng cho "một việc đã xảy ra"; lấy dữ liệu ngay thì dùng query/service interface.
- `ProjectSession`/`IProjectStorage` mới chỉ ở mức **contract**; implementation storage thuộc
  `ProjectManager` (ngoài phạm vi Host v1).
```

---

# Mô hình kiến trúc (giải thích chi tiết)

## 1. Ý tưởng cốt lõi: "Vỏ ứng dụng + Component cắm vào slot"

Hình dung Host như một **bảng điện có sẵn các ổ cắm (slot)**. Host tự nó **trống rỗng về nghiệp vụ** —
nó chỉ vẽ khung cửa sổ và chừa ra 5 ổ cắm. **Module** là các **component** (thiết bị) tự cắm vào ổ.
Host không biết bên trong component làm gì; component không cần sửa Host để xuất hiện trên màn hình.

```
┌───────────────────────── MainWindow (Host = cái vỏ) ─────────────────────────┐
│  Menu (File / Edit←command)      ┌── Toolbar slot ──────────────────────────┐ │
│                                  │  [command] [command]  [ToolbarViews...]  │ │
│                                  └──────────────────────────────────────────┘ │
│ ┌── LeftPanel slot ──┐ ┌──────────── MainCanvas slot ───────────────────────┐ │
│ │                    │ │                                                    │ │
│ │  SampleLeftView    │ │   SampleCanvasView   ← đây là "component" của bạn  │ │
│ │  (component)       │ │   (UserControl + ViewModel)                        │ │
│ │                    │ │                                                    │ │
│ └────────────────────┘ └────────────────────────────────────────────────────┘ │
│ ┌───────────────────────── BottomPanel slot ─────────────────────────────────┐ │
│ └────────────────────────────────────────────────────────────────────────────┘ │
│  StatusBar slot: [StatusText] [StatusBarViews...]                              │
└────────────────────────────────────────────────────────────────────────────────┘
```

5 slot có sẵn (enum `UiSlot`): **Toolbar, LeftPanel, MainCanvas, BottomPanel, StatusBar**.
Ngoài ra có 2 "location" cho command: **`toolbar.main`** và **`menu.edit`**.

## 2. Ba tầng project và ranh giới của chúng

| Project | Vai trò | Được biết gì | KHÔNG được biết gì |
|---|---|---|---|
| `FTDNG.Contracts` | **Hợp đồng** — interface & kiểu dữ liệu chung. UI-free (không WPF). | `IFtdngModule`, `IUiRegistry`, `UiSlot`, `IEventBus`, events | Bất kỳ implementation nào |
| `FTDNG.SharedKernel` | **Primitive** dùng chung, UI-free. | `ProjectId`, `TimeRange`, `Result` | WPF, nghiệp vụ |
| `FTDNG.Host.Wpf` | **Vỏ + Composition Root** (WinExe). Dựng cửa sổ, DI, render slot. | "Có module" qua `IFtdngModule` | Nghiệp vụ/ViewModel cụ thể của module |
| `FTDNG.Modules.*.Wpf` | **Component/plugin**. Chứa View + ViewModel + service riêng. | Contracts + SharedKernel | Host, module khác (chỉ giao tiếp qua EventBus/interface) |

**Quy tắc phụ thuộc (mũi tên = "reference tới"):**

```
Host ──► Contracts ◄── Modules          (Host và Module cùng phụ thuộc Contracts)
Host ──► SharedKernel ◄── Modules
Host ─ ─X─► Modules   (KHÔNG reference output assembly — nạp động lúc runtime)
```

Host **không** reference DLL của module (`ReferenceOutputAssembly=false`). Lúc build, DLL module được
copy vào thư mục `Modules/` cạnh file `.exe`; `ModuleCatalog` quét thư mục đó lúc chạy và tìm class
hiện thực `IFtdngModule` qua reflection. → Thêm/bớt module không phải sửa/biên dịch lại Host.

## 3. Vai trò từng file

**Host — `src/Host/FTDNG.Host.Wpf/`**

| File | Làm gì |
|---|---|
| `App.xaml.cs` | **Composition Root**. `OnStartup` chạy 6 bước: tạo DI → `ModuleCatalog` phát hiện module → mỗi module `RegisterServices` → `BuildServiceProvider` → mỗi module `RegisterUi` → hiện `MainWindow`. |
| `Bootstrap/ModuleCatalog.cs` | Quét assembly đã nạp + thư mục `Modules/`, tìm type `IFtdngModule` (có constructor rỗng), khởi tạo qua reflection. |
| `Bootstrap/ServiceRegistration.cs` | Đăng ký hạ tầng Host vào DI (`EventBus`, `UiRegistry`, `MainWindow`, `MainWindowViewModel`). |
| `Bootstrap/EventBus.cs` | Bus sự kiện in-process, thread-safe — cho module giao tiếp mà không tham chiếu nhau ("việc đã xảy ra"). |
| `Bootstrap/QueryBus.cs` | Bus "hỏi-lấy dữ liệu ngay", in-process, thread-safe. Mỗi cặp (query, result) chỉ **một** handler (một nguồn dữ liệu); handler lỗi không làm sập lời gọi. Đăng ký singleton qua DI. |
| `Shell/MainWindow.xaml` | **Layout gốc của cả app**: `DockPanel` chia 5 vùng, mỗi vùng là một `ItemsControl` bind vào một danh sách view của slot. |
| `Shell/MainWindow.xaml.cs` | Code-behind mỏng: nhận VM qua DI, gọi `Compose()` khi `Loaded`. |
| `Shell/MainWindowViewModel.cs` | Đọc `UiRegistry`, **resolve View qua DI** rồi đổ vào các `ObservableCollection` của từng slot (`LeftPanelViews`, `MainCanvasViews`, ...). |
| `Shell/UiRegistry.cs` | "Sổ đăng ký": lưu *"view/command nào ở slot/location nào"* + thứ tự `Order`. Không chứa nghiệp vụ. |
| `Shell/RelayCommand.cs` | `ICommand` bọc delegate cho command trên toolbar/menu. |

**Contracts — `src/Shared/FTDNG.Contracts/`**

| File | Làm gì |
|---|---|
| `Modules/IFtdngModule.cs` | Hợp đồng mọi module phải hiện thực: `Id`, `RegisterServices`, `RegisterUi`. |
| `Modules/IUiRegistry.cs` | API để module khai báo `AddView(slot, viewType, order)` và `AddCommand(location, descriptor)`. |
| `Modules/UiSlot.cs` | Enum 5 slot. |
| `Modules/UiCommandDescriptor.cs` | Metadata + delegate của một command (UI-neutral, không mang WPF control). |
| `Events/IEventBus.cs`, `Events/IntegrationEvents.cs` | Bus "báo việc đã xảy ra" + các event tích hợp (vd `ProjectOpenedEvent`). |
| `Queries/IQueryBus.cs` | Bus "hỏi-lấy dữ liệu ngay" (đối xứng EventBus). Mỗi query một nguồn dữ liệu; impl `QueryBus` ở Host. |
| `Project/ProjectSession.cs` | Contract phiên làm việc/lưu trữ (chưa có implementation). |

**SharedKernel — `src/Shared/FTDNG.SharedKernel/`** (primitive UI-free, cực kỳ ổn định)

| File | Làm gì |
|---|---|
| `Ids.cs` | Các **strongly-typed ID** dùng chung (`ProjectId`, `CalendarId`, `TaskRowId`, `BarId`, `DependencyId`) — `readonly record struct` bọc `Guid`, so sánh theo giá trị, có `New()`. Không mang nghiệp vụ. |
| `TimeRange.cs` | Khoảng thời gian half-open `[Start, End)` (`DateTimeOffset`), tự kiểm tra `End ≥ Start`; có `Duration`, `Contains`, `Overlaps`. |
| `Result.cs` | `Error` (Code+Message) + `Result`/`Result<T>` — trả kết quả có-thể-thất-bại **không ném exception** cho luồng bình thường. |

**Module mẫu — `src/Modules/Sample/FTDNG.Modules.Sample.Wpf/`** ← *đây là mẫu để bạn code theo*

| File | Làm gì |
|---|---|
| `SampleModule.cs` | Điểm vào của component. `RegisterServices` đăng ký View+ViewModel vào DI; `RegisterUi` khai báo view nằm slot nào + thêm command. |
| `Views/SampleLeftView.xaml(.cs)` | **Một "component" giao diện** (UserControl) hiển thị ở LeftPanel. |
| `Views/SampleCanvasView.xaml(.cs)` | Component ở MainCanvas: có nút phát `ProjectOpenedEvent`. |
| `SampleViewModel.cs` | Nghiệp vụ/state của component (MVVM). Nhận `IEventBus` qua DI. |

## 4. 👉 Bạn code giao diện / "component" ở ĐÂU?

**Bạn KHÔNG sửa Host.** Giao diện được viết **bên trong project module**, dạng **UserControl (View) + ViewModel** —
đúng mẫu MVVM. Mỗi UserControl chính là một "component". Layout tổng thể (5 slot) đã cố định trong
`MainWindow.xaml`; bạn chỉ *cắm* component của mình vào slot mong muốn.

### Cách thêm một component mới vào một module có sẵn (vd Sample)

**B1 — Tạo View (component UI):** `Views/MyPanelView.xaml`
```xml
<UserControl x:Class="FTDNG.Modules.Sample.Wpf.Views.MyPanelView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel Margin="16">
        <TextBlock Text="{Binding Heading}" FontWeight="Bold" />
        <Button Content="Bấm tôi" Command="{Binding DoStuffCommand}" />
    </StackPanel>
</UserControl>
```
Code-behind `Views/MyPanelView.xaml.cs` (nhận ViewModel qua DI, gán `DataContext`):
```csharp
public partial class MyPanelView : UserControl
{
    public MyPanelView(MyPanelViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
```

**B2 — Viết ViewModel** (`MyPanelViewModel.cs`): chứa state + logic, `INotifyPropertyChanged`,
nhận dependency (vd `IEventBus`) qua constructor.

**B3 — Đăng ký vào module** (`SampleModule.cs`):
```csharp
public void RegisterServices(IServiceCollection services)
{
    services.AddTransient<MyPanelView>();       // View
    services.AddSingleton<MyPanelViewModel>();  // ViewModel
}

public void RegisterUi(IUiRegistry ui)
{
    // Cắm component vào slot muốn hiển thị:
    ui.AddView(UiSlot.LeftPanel, typeof(MyPanelView), order: 200);
}
```
Xong. Chạy lại app, component xuất hiện ở LeftPanel. `order` nhỏ hiển thị trước.

### Cách tạo hẳn một module (component lớn) mới

1. Tạo project `src/Modules/<Tên>/FTDNG.Modules.<Tên>.Wpf` (WPF class library, `net8.0-windows`,
   `UseWPF=true`), reference `FTDNG.Contracts` + `FTDNG.SharedKernel`.
2. Tạo class `<Tên>Module : IFtdngModule` (constructor rỗng — để `ModuleCatalog` khởi tạo được).
3. Thêm 1 dòng vào `FTDNG.Host.Wpf.csproj` trong nhóm `<FtdngModuleProject Include="..." />`
   để DLL được build & copy vào `Modules/` (không tạo coupling compile-time).

### Nguyên tắc khi code UI (đang được giữ trong repo)
- **Layout khung** (chia 5 vùng) ở `MainWindow.xaml`; **nội dung từng vùng** ở các UserControl của module.
- View chỉ nhận dependency qua **constructor DI**, gán `DataContext = viewModel`; **không** `new ViewModel` bằng tay.
- Nghiệp vụ nằm ở **ViewModel/service trong module**, không nhét vào Host.
- Muốn giao tiếp giữa các module: phát/nhận qua **`IEventBus`** (việc đã xảy ra) hoặc hỏi-lấy dữ liệu ngay qua **`IQueryBus`** (mỗi query một nguồn dữ liệu).
