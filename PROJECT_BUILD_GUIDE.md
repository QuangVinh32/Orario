# FTDNG Kotei — Hướng dẫn Trình tự Xây dựng Project

**Phiên bản:** 0.0.0-beta | **.NET:** 8.0 | **UI Framework:** WPF

---

## 📋 Mục lục

1. [Tổng quan kiến trúc](#tổng-quan-kiến-trúc)
2. [Trình tự build](#trình-tự-build)
3. [Sơ đồ dependencies](#sơ-đồ-dependencies)
4. [Chi tiết từng project](#chi-tiết-từng-project)
5. [Hướng dẫn build](#hướng-dẫn-build)
6. [Luồng bootstrap runtime](#luồng-bootstrap-runtime)

---

## 🏗️ Tổng quan kiến trúc

Project sử dụng kiến trúc **Modular Monolith** với các đặc điểm:

- **Loose Coupling**: Module không tham chiếu nhau (chỉ qua Interface/EventBus)
- **Dynamic Loading**: Module được nạp động từ thư mục `Modules/` lúc runtime
- **Layered Architecture**:
  - **Shared Layer** (UI-free): SharedKernel, Contracts
  - **Module Layer** (WPF Modules): Sample, Notifier, Counter
  - **Host/Shell Layer** (Main App): Host.Wpf

---

## 🔨 Trình tự Build

### **Thứ tự xây dựng (từ dưới lên):**

```
┌─────────────────────────────────────────┐
│ Bước 1: FTDNG.SharedKernel              │
│ ├─ Không phụ thuộc project nào          │
│ └─ Cơ sở cho tất cả project khác        │
└─────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│ Bước 2: FTDNG.Contracts                 │
│ ├─ Phụ thuộc: SharedKernel              │
│ └─ Chứa interfaces & contracts          │
└─────────────────────────────────────────┘
                   ↓
        ┌──────────────────┐
        │   Song song      │
        │  (Bước 3)        │
        └──────────────────┘
    ↙           ↓           ↘
┌──────────┐ ┌──────────┐ ┌──────────┐
│ Sample   │ │Notifier  │ │ Counter  │
│ Module   │ │ Module   │ │ Module   │
│ (Wpf)    │ │ (Wpf)    │ │ (Wpf)    │
│ ← Phụ: SK│ │ ← Phụ: SK│ │ ← Phụ: SK│
│ ← Phụ: C │ │ ← Phụ: C │ │ ← Phụ: C │
└──────────┘ └──────────┘ └──────────┘
    ↘           ↓           ↙
        ┌──────────────────┐
        │   Bước 4         │
        │ FTDNG.Host.Wpf   │
        │ ← Phụ: SK, C     │
        │ Nạp module DLL   │
        └──────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│ Bước 5: Tests (Optional)                │
│ ├─ FTDNG.Modules.Notifier.Wpf.Tests     │
│ └─ Phụ thuộc module & contracts         │
└─────────────────────────────────────────┘
```

### **Thứ tự chi tiết:**

| Thứ tự | Project | Loại | Phụ thuộc | Ghi chú |
|--------|---------|------|-----------|---------|
| **1** | `FTDNG.SharedKernel` | Library | _Không_ | Cơ sở, không UI |
| **2** | `FTDNG.Contracts` | Library | SharedKernel | Interfaces, UI-neutral |
| **3a** | `FTDNG.Modules.Sample.Wpf` | Library | SharedKernel, Contracts | Có thể build song song |
| **3b** | `FTDNG.Modules.Notifier.Wpf` | Library | SharedKernel, Contracts | Có thể build song song |
| **3c** | `FTDNG.Modules.Counter.Wpf` | Library | SharedKernel, Contracts | Có thể build song song |
| **4** | `FTDNG.Host.Wpf` | WinExe (Main) | SharedKernel, Contracts | Sao chép Module DLL |
| **5** | `FTDNG.Modules.Notifier.Wpf.Tests` | Test Project | (Test) | Tuỳ chọn |

---

## 📊 Sơ đồ Dependencies

```
FTDNG.SharedKernel (Cơ sở, không dependency)
│
├──→ FTDNG.Contracts
│    └──→ IEventBus
│    └──→ IFtdngModule
│    └──→ IUiRegistry
│    └──→ IntegrationEvents
│
├──→ FTDNG.Modules.Sample.Wpf
│    ├──→ SharedKernel
│    └──→ Contracts (lấy IFtdngModule)
│
├──→ FTDNG.Modules.Notifier.Wpf
│    ├──→ SharedKernel
│    └──→ Contracts (lấy IFtdngModule)
│
├──→ FTDNG.Modules.Counter.Wpf
│    ├──→ SharedKernel
│    └──→ Contracts (lấy IFtdngModule)
│
└──→ FTDNG.Host.Wpf (WinExe - Ứng dụng chính)
     ├──→ SharedKernel
     ├──→ Contracts
     ├──→ Modules/* (ReferenceOutputAssembly=false ⚠️)
     │   └─→ Copy DLL vào thư mục Modules/ lúc Build
     └──→ Dependency Injection, WPF, SharpVectors
```

### **Quan trọng: Loose Coupling trong Host**

```csharp
<!-- Host.csproj -->
<ItemGroup>
    <!-- Các module được liệt kê nhưng KHÔNG lấy output assembly -->
    <ProjectReference Include="Modules/*/...csproj"
                      ReferenceOutputAssembly="false"
                      Private="false" />
</ItemGroup>

<!-- Custom Build Target -->
<Target Name="CopyFtdngModules" AfterTargets="Build">
    <!-- Build module → Copy DLL vào $(OutDir)Modules/ -->
    <!-- Host KHÔNG dependency compile-time vào module DLL -->
</Target>
```

**Lợi ích:**
- Modules độc lập, có thể thêm/xoá mà không thay đổi Host
- Nạp module động lúc runtime từ thư mục `Modules/`
- Zero compile-time coupling giữa Host và Module

---

## 🎯 Chi tiết từng project

### **1. FTDNG.SharedKernel**

**Đường dẫn:** `src/Shared/FTDNG.SharedKernel/`

**Mục đích:**
- Các primitive type không UI
- Nền tảng cho tất cả project khác

**Nội dung:**
- `Ids.cs` — ID abstractions
- `Result.cs` — Result/Error pattern
- `TimeRange.cs` — Date range utilities

**Dependencies:** Không

**Constraint:** ❌ Cấm WPF, System.Windows, UI bất kỳ

**TargetFramework:** `net8.0`

---

### **2. FTDNG.Contracts**

**Đường dẫn:** `src/Shared/FTDNG.Contracts/`

**Mục đích:**
- Interfaces & contracts giữa Host ↔ Module
- Kế hoạch project (ProjectSession)
- Events integration

**Nội dung:**
- `Events/IEventBus.cs` — Event broker interface
- `Events/IntegrationEvents.cs` — Business events
- `Modules/IFtdngModule.cs` — **Module phải implement**
- `Modules/IUiRegistry.cs` — UI registry interface
- `Modules/UiCommandDescriptor.cs` — UI command metadata
- `Modules/UiSlot.cs` — Slot definition (Toolbar, Canvas, etc.)
- `Queries/IQueryBus.cs` — Query interface
- `Queries/DemoQueries.cs` — Example queries
- `Project/ProjectSession.cs` — Project context

**Dependencies:**
- `FTDNG.SharedKernel`
- `Microsoft.Extensions.DependencyInjection.Abstractions`

**Constraint:** ❌ Cấm WPF, System.Windows

**TargetFramework:** `net8.0`

---

### **3. Modules (Song song — Bước 3a, 3b, 3c)**

#### **3a. FTDNG.Modules.Sample.Wpf**

**Đường dẫn:** `src/Modules/Sample/FTDNG.Modules.Sample.Wpf/`

**Mục đích:** Module demo chứng minh pipeline complète (DI → UiRegistry → EventBus)

**Nội dung:**
- `SampleModule.cs` — Implement `IFtdngModule`
- `SampleViewModel.cs` — View Model
- `Views/` — XAML Views

**Dependencies:**
- `FTDNG.SharedKernel`
- `FTDNG.Contracts`

**TargetFramework:** `net8.0-windows`

**UseWPF:** `true`

---

#### **3b. FTDNG.Modules.Notifier.Wpf**

**Đường dẫn:** `src/Modules/Notifier/FTDNG.Modules.Notifier.Wpf/`

**Mục đích:** Module thứ 2, chứng minh 2+ module giao tiếp qua EventBus mà không reference nhau

**Nội dung:**
- `NotifierModule.cs` — Implement `IFtdngModule`
- `NotifierViewModel.cs` — View Model
- `Views/` — XAML Views

**Dependencies:**
- `FTDNG.SharedKernel`
- `FTDNG.Contracts`

**TargetFramework:** `net8.0-windows`

**UseWPF:** `true`

---

#### **3c. FTDNG.Modules.Counter.Wpf**

**Đường dẫn:** `src/Modules/Counter/FTDNG.Modules.Counter.Wpf/`

**Mục đích:** Module tối giản, ví dụ "Hello World" module

**Nội dung:**
- `CounterModule.cs` — Implement `IFtdngModule`
- `CounterViewModel.cs` — View Model
- `Views/` — XAML Views

**Dependencies:**
- `FTDNG.SharedKernel`
- `FTDNG.Contracts`

**TargetFramework:** `net8.0-windows`

**UseWPF:** `true`

---

### **4. FTDNG.Host.Wpf**

**Đường dẫn:** `src/Host/FTDNG.Host.Wpf/`

**Mục đích:** Ứng dụng chính (Composition Root)

**Nội dung:**
- `App.xaml(.cs)` — Composition Root, Bootstrap 6 bước
- `Bootstrap/`
  - `ServiceRegistration.cs` — DI setup
  - `ModuleCatalog.cs` — Phát hiện module
  - `EventBus.cs` — Event broker in-process
  - `QueryBus.cs` — Query handler
- `Shell/`
  - `MainWindow.xaml(.cs)` — 5 UI slot
  - `MainWindowViewModel.cs` — Shell ViewModel
  - `UiRegistry.cs` — Implements `IUiRegistry`
  - `RelayCommand.cs` — ICommand helper
- `Assets/` — SVG icons

**Dependencies:**
- `FTDNG.SharedKernel`
- `FTDNG.Contracts`
- ⚠️ Modules (ReferenceOutputAssembly=false)

**Output Type:** `WinExe` (Executable)

**TargetFramework:** `net8.0-windows`

**UseWPF:** `true`

**NuGet Packages:**
- `Microsoft.Extensions.DependencyInjection` (8.0.1)
- `Microsoft.Extensions.Logging` (8.0.1)
- `Microsoft.Extensions.Logging.Debug` (8.0.1)
- `SharpVectors.Wpf` (1.8.4) — Render SVG trong WPF

**Custom Build Target:**
```csharp
<Target Name="CopyFtdngModules" AfterTargets="Build">
    <!-- Build module → Copy DLL vào $(OutDir)Modules/ -->
</Target>
```

---

### **5. FTDNG.Modules.Notifier.Wpf.Tests** (Optional)

**Đường dẫn:** `tests/FTDNG.Modules.Notifier.Wpf.Tests/`

**Mục đích:** Unit tests cho Notifier module

**Dependencies:** Notifier module, test framework

---

## 🚀 Hướng dẫn Build

### **Cách 1: Build toàn bộ solution (Visual Studio)**

```bash
cd d:\New folder\Orario
dotnet build FTDNG.sln --configuration Debug
```

**Kết quả:**
- `bin/Debug/net8.0-windows/FTDNG.Host.Wpf.exe` → ứng dụng chính
- `bin/Debug/net8.0-windows/Modules/` → Module DLL:
  - `FTDNG.Modules.Sample.Wpf.dll`
  - `FTDNG.Modules.Notifier.Wpf.dll`
  - `FTDNG.Modules.Counter.Wpf.dll`

### **Cách 2: Build project cụ thể**

```bash
# Build SharedKernel trước
dotnet build src/Shared/FTDNG.SharedKernel/FTDNG.SharedKernel.csproj

# Build Contracts
dotnet build src/Shared/FTDNG.Contracts/FTDNG.Contracts.csproj

# Build Module
dotnet build src/Modules/Sample/FTDNG.Modules.Sample.Wpf/FTDNG.Modules.Sample.Wpf.csproj

# Build Host (tự động sao chép Module DLL)
dotnet build src/Host/FTDNG.Host.Wpf/FTDNG.Host.Wpf.csproj
```

### **Cách 3: Rebuild từ đầu**

```bash
dotnet clean FTDNG.sln
dotnet build FTDNG.sln --configuration Debug
```

### **Cách 4: Chạy ứng dụng**

```bash
# Chạy Host.Wpf
dotnet run --project src/Host/FTDNG.Host.Wpf/FTDNG.Host.Wpf.csproj
```

---

## 🔄 Luồng Bootstrap Runtime (Mục 4.2)

### **App.OnStartup() — 6 bước**

```
┌─────────────────────────────────────────┐
│ Bước 1: Khởi tạo ServiceCollection      │
│ + EventBus, UiRegistry, Shell           │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Bước 2: ModuleCatalog.Discover()        │
│ Tìm IFtdngModule từ:                    │
│ - Loaded assemblies                     │
│ - Modules/ folder                       │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Bước 3: Module.RegisterServices()       │
│ Từng module đăng ký:                    │
│ - ViewModels                            │
│ - Services                              │
│ - Handlers                              │
│ vào DI container                        │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Bước 4: BuildServiceProvider()          │
│ Xây dựng DI container hoàn chỉnh        │
│ (sau khi all dependencies registered)   │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Bước 5: Module.RegisterUi()             │
│ Từng module đăng ký UI:                 │
│ - View (WPF UserControl)                │
│ - Command (button, menu)                │
│ vào UiRegistry (theo slot)              │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│ Bước 6: MainWindow Render               │
│ Host đọc UiRegistry, resolve View qua   │
│ DI, ghép từng View vào slot tương ứng   │
│                                          │
│ Kết quả: Mainwindow với UI module       │
└─────────────────────────────────────────┘
```

### **Các slot trong Shell**

[MainWindow.xaml](./src/Host/FTDNG.Host.Wpf/Shell/MainWindow.xaml) có 5 slot:

```xaml
┌─────────────────────────────────┐
│        TOOLBAR (top)            │  ← UiSlot.Toolbar
├─────┬─────────────────┬─────────┤
│     │                 │         │
│LEFT │                 │ RIGHT   │  ← UiSlot.LeftPanel, MainCanvas, RightPanel
│PANEL│  MAIN CANVAS    │ PANEL   │
│     │                 │         │
│     │                 │         │
├─────┴─────────────────┴─────────┤
│    BOTTOM PANEL (bottom)        │  ← UiSlot.BottomPanel
├─────────────────────────────────┤
│    STATUS BAR (very bottom)     │  ← UiSlot.StatusBar
└─────────────────────────────────┘
```

Module có thể đăng ký UI vào bất kỳ slot nào qua `RegisterUi()`.

---

## 📝 Ghi chú quan trọng

### **✅ PHẢI làm**

1. **Build theo thứ tự**: SharedKernel → Contracts → Modules → Host
2. **Kế thừa `IFtdngModule`** trong module:
   ```csharp
   public class SampleModule : IFtdngModule
   {
       public void RegisterServices(IServiceCollection services) { }
       public void RegisterUi(IUiRegistry uiRegistry) { }
   }
   ```
3. **Đặt module class public** để ModuleCatalog tìm được
4. **Module DLL phải** trong thư mục `Modules/` lúc runtime

### **❌ KHÔNG được làm**

1. ❌ Reference Contracts trực tiếp vào codebase (chỉ interface)
2. ❌ Module reference nhau (giao tiếp qua EventBus)
3. ❌ SharedKernel/Contracts có UI (WPF, System.Windows)
4. ❌ Build Host trước khi Contracts xong
5. ❌ Module reference Host (cyclic dependency)

### **⚠️ Lưu ý Loose Coupling**

```csharp
// ❌ SAI: Module A reference Module B trực tiếp
using FTDNG.Modules.Notifier.Wpf;
notifier.SendMessage(); // coupling

// ✅ ĐÚNG: Module A gửi event, Module B subscribe
eventBus.Publish(new MessageSentEvent());
```

---

## 🔗 File liên quan

| Loại | Đường dẫn | Mục đích |
|------|----------|---------|
| Solution | [FTDNG.sln](./FTDNG.sln) | VS solution file |
| Config | [global.json](./global.json) | SDK .NET version |
| Docs | [README.md](./README.md) | Project overview |
| Boot | [App.xaml.cs](./src/Host/FTDNG.Host.Wpf/App.xaml.cs) | Bootstrap code |
| Module | [SampleModule.cs](./src/Modules/Sample/FTDNG.Modules.Sample.Wpf/SampleModule.cs) | Module example |
| Contracts | [IFtdngModule.cs](./src/Shared/FTDNG.Contracts/Modules/IFtdngModule.cs) | Module interface |

---

## 📚 Tham khảo

- **DDD (Domain-Driven Design)**: SharedKernel, Contracts
- **MVVM**: WPF ViewModel pattern
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Event-Driven**: EventBus for inter-module communication
- **Modular Monolith**: Dynamic module loading

---

**Created:** 2026-08-17 | **Version:** 0.0.0-beta
