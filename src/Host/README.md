# FTDNG.Host.Wpf — Kiến trúc Host & vai trò từng thư mục/file

> **Một câu tóm tắt:** Host là **cái vỏ (Shell)** + **bộ ghép nối (Composition Root)**.
> Nó dựng cửa sổ, tìm module, cho module tự đăng ký, rồi render UI của module vào các "chỗ trống" (slot).
> Host **không** chứa nghiệp vụ (Calendar/WBS/Gantt…) — nghiệp vụ nằm trong các Module.

---

## 1. Bức tranh tổng thể

```
                 ┌─────────────────────────────────────────────┐
                 │              FTDNG.Host.Wpf                 │
                 │   Composition Root + Shell (cái vỏ)         │
                 │                                             │
   App khởi động │   App.OnStartup:                            │
   ───────────►  │   (1) Đăng ký hạ tầng Host vào DI           │
                 │   (2) ModuleCatalog quét & tìm module       │
                 │   (3) module.RegisterServices(DI)           │
                 │   (4) BuildServiceProvider                  │
                 │   (5) module.RegisterUi(UiRegistry)         │
                 │   (6) MainWindow đọc UiRegistry → render     │
                 └───────────────┬─────────────────────────────┘
                                 │  chỉ giao tiếp qua "hợp đồng"
                                 ▼
        ┌────────────────────────────────────────────────┐
        │   FTDNG.Contracts  (interface/kiểu dùng chung)  │
        │   IFtdngModule, IUiRegistry, UiSlot,            │
        │   IEventBus, IQueryBus, UiCommandDescriptor…    │
        └────────────────────────────────────────────────┘
                                 ▲
                                 │  hiện thực hợp đồng
        ┌────────────────────────┴───────────────────────┐
        │   Các Module (nạp động qua DLL)                 │
        │   vd: FTDNG.Modules.Sample.Wpf                  │
        │   → chứa nghiệp vụ + View + ViewModel           │
        └─────────────────────────────────────────────────┘
```

**Nguyên tắc vàng:** Host **biết là "có module"** nhưng **không biết module làm gì**.
Mọi liên lạc đi qua các interface trong `FTDNG.Contracts`, nên Host không tham chiếu
compile-time tới bất kỳ Module cụ thể nào.

---

## 2. Vòng đời khởi động (đọc `App.xaml.cs`)

| Bước | Việc xảy ra | Ai làm |
|------|-------------|--------|
| 1 | Tạo `ServiceCollection`, đăng ký EventBus/QueryBus/UiRegistry/Shell | `ServiceRegistration.AddHostInfrastructure` |
| 2 | Quét assembly, tìm class hiện thực `IFtdngModule` | `ModuleCatalog.DiscoverModules` |
| 3 | Mỗi module tự đăng ký service/ViewModel của nó | `module.RegisterServices(services)` |
| 4 | `BuildServiceProvider()` — giờ mới có DI đầy đủ | Host |
| 5 | Mỗi module khai báo View/Command vào sổ đăng ký UI | `module.RegisterUi(uiRegistry)` |
| 6 | `MainWindow` đọc `UiRegistry`, resolve View qua DI, ghép vào slot | Shell |

Thứ tự này quan trọng: **RegisterServices trước khi build DI**, **RegisterUi sau khi build DI**
(để View có sẵn dependency khi được resolve).

---

## 3. Vai trò từng thư mục / file

### 📄 Gốc project
| File | Vai trò |
|------|---------|
| `FTDNG.Host.Wpf.csproj` | Cấu hình build. Khai báo NuGet (DI, Logging, **SharpVectors** để render .svg), danh sách **Module nạp động** (`FtdngModuleProject`) và target copy DLL module vào thư mục `Modules/`. |
| `App.xaml` / `App.xaml.cs` | **Composition Root**. `App.xaml` giữ Resource toàn cục (merge `Assets/Icons.xaml`). `App.xaml.cs` chạy 6 bước khởi động ở Mục 2. |

### 📁 `Bootstrap/` — hạ tầng Host (không nghiệp vụ)
| File | Vai trò |
|------|---------|
| `ServiceRegistration.cs` | Hàm mở rộng `AddHostInfrastructure`: đăng ký các service **do Host sở hữu** (EventBus, QueryBus, UiRegistry, ModuleCatalog, Shell) vào DI. |
| `ModuleCatalog.cs` | **Bộ dò module**. Quét assembly đã nạp + DLL trong thư mục `Modules/`, tìm class hiện thực `IFtdngModule`, khởi tạo instance. Đây là chỗ "phát hiện plugin". |
| `EventBus.cs` | Hiện thực `IEventBus` — **pub/sub** in-process, thread-safe. Dùng để thông báo *"một việc đã xảy ra"* (fire-and-forget, nhiều người nghe). |
| `QueryBus.cs` | Hiện thực `IQueryBus` — **request/response** in-process. Dùng để *"hỏi và lấy dữ liệu ngay"* (mỗi query đúng 1 nguồn trả lời). |

> EventBus vs QueryBus: **Event** = báo tin (0..n handler, không chờ trả lời).
> **Query** = hỏi dữ liệu (đúng 1 handler, có kết quả). Cả hai giúp module nói chuyện với nhau
> **mà không tham chiếu trực tiếp** vào nhau.

### 📁 `Shell/` — cái vỏ hiển thị (MVVM)
| File | Vai trò |
|------|---------|
| `MainWindow.xaml` | Bố cục cửa sổ: Menu, Toolbar (các nút + **icon SVG**), và 5 vùng slot (Toolbar/LeftPanel/MainCanvas/BottomPanel/StatusBar) dưới dạng `ItemsControl` bind vào ViewModel. |
| `MainWindow.xaml.cs` | Code-behind **chỉ lo phần vỏ**: nhận ViewModel qua DI, gọi `Compose()`, xử lý sự kiện UI thuần (đóng cửa sổ, cuộn toolbar, ẩn nút overflow). Không có nghiệp vụ. |
| `MainWindowViewModel.cs` | ViewModel của Shell. `Compose()` đọc `UiRegistry` → resolve View qua DI → đổ vào các `ObservableCollection` của từng slot; bọc Command của module thành `ICommand`. |
| `UiRegistry.cs` | **Sổ đăng ký UI**. Lưu "ai gắn View/Command nào vào slot/location nào" (kèm thứ tự). Hiện thực `IUiRegistry`. Chỉ là dữ liệu, không render. |
| `RelayCommand.cs` | `ICommand` tối giản — "dây nối" giữa nút WPF và delegate `Execute/CanExecute` do module cung cấp. |

### 📁 `Assets/` — tài nguyên hình ảnh
| Mục | Vai trò |
|------|---------|
| `Icons.xaml` | ResourceDictionary chứa icon dạng **Geometry** (path vẽ tay / placeholder) cho các nút chưa có file SVG. Được merge trong `App.xaml`. |
| `SVG_icon/Actions/*.svg` | Icon **file SVG thật** cho toolbar (new, open, undo, align…). Render bằng `<svgc:SvgViewbox Source="…"/>` (SharpVectors). Được nhúng dạng `Resource` qua wildcard trong csproj. |
| `SVG_icon/Symbol/**` | Bộ ký hiệu (mũi tên, hình tròn/tam giác/ngũ giác… solid & outline) dùng cho biểu diễn dữ liệu trên canvas sau này. |

---

## 4. Ranh giới trách nhiệm (điều Host KHÔNG được làm)

- ❌ Không `new` ViewModel/View của module bằng tay → luôn **resolve qua DI**.
- ❌ Không tham chiếu compile-time tới project Module (chỉ dùng để đảm bảo thứ tự build,
  `ReferenceOutputAssembly=false`).
- ❌ Không đặt logic nghiệp vụ (Calendar/WBS/Gantt) trong `Shell/` hay `Bootstrap/`.
- ✅ Host chỉ: dựng vỏ, tìm module, ghép DI, render slot, cung cấp EventBus/QueryBus.

---

## 5. Muốn thêm một Module mới thì làm gì?

1. Tạo project `FTDNG.Modules.<Tên>.Wpf`, hiện thực `IFtdngModule`.
2. Trong `RegisterServices`: đăng ký ViewModel/service của module vào DI.
3. Trong `RegisterUi`: gọi `ui.AddView(UiSlot.X, typeof(MyView))` và/hoặc
   `ui.AddCommand("toolbar.main", new UiCommandDescriptor{…})`.
4. Thêm 1 dòng `<FtdngModuleProject Include="…"/>` trong `FTDNG.Host.Wpf.csproj`
   (Host sẽ tự build & copy DLL vào `Modules/`).
5. Chạy Host — `ModuleCatalog` tự phát hiện, UI của module tự xuất hiện ở slot đã khai báo.

> Không cần sửa code Host cho từng module → đúng tinh thần **plugin/mở rộng**.

---

## 6. Bản đồ nhanh "cần sửa gì thì vào đâu"

| Muốn… | Vào file |
|-------|----------|
| Thêm/bớt service hạ tầng chung | `Bootstrap/ServiceRegistration.cs` |
| Đổi cách dò module | `Bootstrap/ModuleCatalog.cs` |
| Thêm slot UI mới | `FTDNG.Contracts/Modules/UiSlot.cs` + `Shell/MainWindow.xaml` + `MainWindowViewModel.cs` |
| Đổi bố cục cửa sổ / toolbar / icon | `Shell/MainWindow.xaml` (+ `Assets/`) |
| Sửa quy tắc render slot | `Shell/MainWindowViewModel.cs` |
| Thêm kiểu event/query dùng chung | `FTDNG.Contracts/Events` hoặc `FTDNG.Contracts/Queries` |
