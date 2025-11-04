## DuckovMods — agent 快速上手说明

目标：为 AI 编码代理提供立即可用的、本仓库可执行的上下文和约定，便于快速完成修改、补丁与功能实现。

- 仓库概览
  - 顶层为多个独立 mod 项目文件夹（例如 `HideCharacter/`, `HitFeedback/`, `SceneSnapshot/`, `Theme/`, `UIFrame/`）。每个模块通常包含一个 `*.csproj`、`ModBehaviour.cs` 与若干源文件。
  - 解决方案文件：`DuckovMods.sln`。README 位于根目录，简短说明了仓库目的。

- 关键约定（必须遵循）
  - 每个 mod 通过类 `ModBehaviour` 实现，且继承自 `Duckov.Modding.ModBehaviour`（示例：`Theme/ModBehaviour.cs`）。常用生命周期钩子：`OnAfterSetup()`、`OnBeforeDeactivate()`。
  - 模块间互操作通常通过反射和“安全 API 包装器”完成（示例：`HideCharacter/Api/ModConfigApi.cs`）。这些包装器：
    - 在运行时用 `AppDomain.CurrentDomain.GetAssemblies()` 扫描目标程序集并反射所需类型/方法；
    - 做好异常捕获并返回布尔成功标志或默认值；
    - 约定配置键名为 `${modName}_${key}`（见 `SafeLoad`/`SafeSave` 用法）。
  - 配置与版本兼容：一些 API 会检查目标 mod 的静态 `VERSION` 字段（例中 `ModConfig.ModBehaviour.VERSION`）。在交互前请优先调用包装器的 `Initialize()`。
  - 日志使用 UnityEngine 的 `Debug.Log` / `Debug.LogWarning` / `Debug.LogError`。

- 构建 / 调试工作流
  - 这是一个 .NET 多项目解决方案。常规命令（PowerShell）：
    - 构建（调试）：`dotnet build d:\vs_project\DuckovMods\DuckovMods.sln`
    - 构建（发布）：`dotnet build d:\vs_project\DuckovMods\DuckovMods.sln -c Release`
  - 注意：部分项目输出见 `bin/Debug/netstandard2.1/` 或 `bin/Release/net9.0/`，在修改目标框架或依赖时，请检查对应 `*.csproj` 的 `<TargetFramework>`。
  - 仓库内未发现独立的单元测试项目 —— 若需要添加测试，请在顶层新建 `tests/` 并使用常见测试框架（xUnit/NUnit）。

- 常见开发任务与示例
  - 添加新 mod：在仓库根添加新文件夹并创建 `YourMod.csproj`、`ModBehaviour.cs`（继承 `Duckov.Modding.ModBehaviour`），在解决方案中加入 csproj。
  - 与 ModConfig 交互：参考 `HideCharacter/Api/ModConfigApi.cs` 的模式，优先编写“安全静态封装”以免反射调用抛出未捕获异常。
  - 读取/保存配置：使用 `SafeLoad<T>(modName, key, default)` 与 `SafeSave<T>(modName, key, value)`，键名需按 `${modName}_${key}` 规则。

- 代码风格与约定（可自动化检查）
  - 命名：模块文件夹以 PascalCase，类名以 PascalCase。
  - 公共跨 mod API 放在各模块的 `Api/` 子目录（例如 `HideCharacter/Api/`），以降低命名冲突。
  - 反射查找尽量包含日志输出以便定位加载顺序问题（本仓库中已有此类日志用法）。

- 易出错点（agent 在修改时应特别留意）
  - 直接调用其他 mod 的内部类型/方法会因加载顺序或版本不兼容而失败——请优先使用或添加“安全包装器”。
  - 修改 TargetFramework 可能导致与 Unity 或已有二进制不兼容。先在本地构建并检查 `bin/` 输出。
  - 不要假设存在单元测试；对行为敏感修改请手动验证或添加小型验证脚本。

- 推荐起始文件（优先阅读）
  - `Theme/ModBehaviour.cs`（mod 生命周期示例）
  - `HideCharacter/Api/ModConfigApi.cs`（反射 + 安全包装器范例）
  - `UIFrame/UIFrameAPI.cs`（公共 API 占位）
  - `DuckovMods.sln` 与根 `README.md`

如果上面有遗漏或需要更详细的“如何运行/调试某个 mod”步骤，请指出具体目标（例如：“在 Unity 中热重载某 mod”或“为 HideCharacter 添加新的 ModConfig 项”），我会把说明扩展成更精确的操作步骤。
