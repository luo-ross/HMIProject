# RS.SetupApp 发布级完善设计

日期：2026-07-10

状态：已批准，等待实施计划

视觉路线：Fluent Workbench

## 1. 目标

本轮把 `RS.SetupApp` 从“可运行的安装器框架”提升为可安全发布、可恢复、可验证、可自动化测试的 Windows 桌面安装器。完成后应同时满足：

1. 安装器不会覆盖或删除不属于当前产品的目录。
2. 安装、更新、修复和卸载均具有明确的事务边界。
3. 用户取消、窗口关闭、进程异常和步骤失败都能进入可观察的恢复流程。
4. 在线更新只能通过 HTTPS 获取，并能验证发布者签名。
5. WPF 界面使用 Fluent Workbench 侧栏向导，准确呈现准备、执行、取消、恢复、失败和完成状态。
6. 核心安全行为由自动化测试证明，不能依赖人工约定。

## 2. 非目标

- 不改造成 MSIX、MSI 或 WiX 项目。
- 不支持非 Windows 平台。
- 不引入云端账号、遥测平台或在线许可证系统。
- 不在仓库中生成或保存发布私钥。
- 不重写现有产品清单体系；在兼容现有清单的前提下扩展安全字段。
- 不重构与安装器无关的 WPF 客户端、服务端或通信库。

## 3. 当前问题

### 3.1 安装目标不安全

模板默认 `allowOverwrite=true`，当前校验允许把任意已有目录整体移动到临时备份。安装成功后临时目录被递归清理，可能永久删除用户原有内容。

### 3.2 回滚与取消耦合

`SetupStepRunner` 把原始、可能已经取消的 `CancellationToken` 传给回滚步骤，而各回滚步骤首先检查取消。这会让“用户取消”直接跳过恢复。回滚步骤又在执行成功后才入栈，步骤内部发生部分写入时无法补偿。

### 3.3 窗口生命周期不受控

WPF 界面执行操作时使用 `CancellationToken.None`，关闭按钮可以直接结束窗口。用户无法安全取消，也无法知道关闭后是否完成恢复。

### 3.4 卸载信任状态文件中的路径

卸载流程直接使用 `InstalledStateManifest` 中的安装目录、数据目录、维护目录和备份目录执行递归删除，未重新归一化并验证归属边界。

### 3.5 更新只有散列，没有发布者身份

更新 feed、包清单和包由同一发布源提供。攻击者若控制该源，可以生成彼此一致的恶意散列。运行时也没有统一拒绝明文 HTTP。

### 3.6 UI 与引擎状态脱节

当前 `MainWindow.xaml` 把所有页面堆叠在一个文件中，主要通过 Visibility 切换。界面只区分忙碌与完成，无法表达取消中、回滚中、恢复失败或可重试状态。

## 4. 总体架构

实现分为四层：

1. **安全策略层**：路径归一化、危险目录识别、产品归属标记、状态路径校验、下载 URI 策略和签名验证。
2. **事务执行层**：操作日志、持久化恢复目录、步骤执行、逆序补偿、崩溃恢复和提交清理。
3. **应用编排层**：安装、更新、修复、卸载流程，负责把引擎状态映射为 UI 状态。
4. **WPF 表现层**：Fluent Workbench 外壳、页面 ViewModel、异步命令、取消/关闭协调、日志和可访问性。

依赖方向保持为 `RS.SetupApp -> RS.SetupApp.Core`。安全策略和事务能力全部放在 Core，WPF 不复制业务规则。

## 5. 安装目录安全设计

### 5.1 产品归属标记

成功安装时在安装根目录写入 `.rs-setup-owner.json`，模型名为 `InstallationOwnershipMarker`，包含：

- `SchemaVersion`
- `ProductId`
- `InstallationId`
- `InstallScope`
- `CreatedAtUtc`

标记不保存秘密，仅证明目录由本安装器管理。外部状态清单同时保存相同的 `InstallationId`。非空目录只有在外部状态与目录标记的 `ProductId`、`InstallationId` 和 scope 全部一致时，才允许更新、修复或覆盖。

### 5.2 路径决策

新增 `SetupPathSafetyPolicy`，所有路径先执行 `Path.GetFullPath` 并使用 Windows 不区分大小写比较。规则如下：

- 永远拒绝驱动器根目录。
- 永远拒绝 Windows 目录及其子目录。
- 永远拒绝 Program Files、ProgramData、用户 Profile、Desktop、Documents、Downloads、AppData 等特殊目录本身；允许其下方的产品子目录。
- 已存在的目标目录若带有重解析点且无法证明最终目标仍位于允许根内，则拒绝。
- 不存在或为空的普通子目录允许首次安装。
- 非空且无匹配归属标记的目录永远拒绝，即使 `allowOverwrite=true`。
- `allowOverwrite` 只控制是否允许替换“已确认属于同一产品”的安装，不再允许覆盖无归属目录。
- AllUsers 安装必须位于允许的机器级根并要求提权；CurrentUser 安装默认位于 LocalAppData 的产品目录。

校验结果使用结构化 `InstallTargetValidationResult`，包含错误代码、用户消息和规范化路径，UI 不解析异常文本来决定状态。

## 6. 事务与恢复设计

### 6.1 持久化事务日志

新增 `SetupTransactionJournal`，使用原子方式写入恢复根目录：先写临时文件，再替换正式文件。字段包括：

- `OperationId`
- `ProductId`
- `Mode`
- `InstallDirectory`
- `BackupDirectory`
- `Phase`
- `CompletedSteps`
- `StartedAtUtc` / `UpdatedAtUtc`
- `PrimaryError`
- `RecoveryErrors`

阶段为：`Prepared`、`SnapshotCreated`、`Applying`、`Verifying`、`Committing`、`Committed`、`RollingBack`、`RolledBack`、`RecoveryFailed`。

### 6.2 恢复目录

备份不再放在普通 working directory 中。新增 `ISystemPaths.GetRecoveryDirectory(productId, operationId, scope)`：

- CurrentUser 使用 LocalAppData 下的产品恢复根。
- AllUsers 使用 ProgramData 下的产品恢复根。

普通临时目录可以随时清理；持久备份和 journal 只有在事务成功提交或回滚完成后才删除。恢复失败时必须保留，以便下次启动继续恢复。

### 6.3 步骤和回滚语义

- `IRollbackStep` 在执行其正向逻辑前登记到回滚栈。
- 所有回滚实现必须幂等；未发生变更时返回成功。
- 正向执行使用用户操作 token。
- 回滚使用独立的恢复 token，默认五分钟超时，不受用户取消影响。
- 原始异常始终保留；回滚错误单独记录并返回，不能覆盖根因。
- 每个有多项副作用的步骤要么在内部逐项登记补偿，要么拆成原子步骤。
- `CommitTransactionStep` 仅在文件验证、状态清单和归属标记全部成功后执行；它负责清理恢复快照和 journal。

### 6.4 崩溃恢复

启动安装器时先扫描当前产品的未完成 journal：

- `Committed` 或 `RolledBack`：清理残留后继续。
- 其他阶段：先进入 Recovery 页面并执行逆序恢复。
- 恢复完成后允许开始新操作。
- 恢复失败时禁止继续安装或卸载，保留日志和快照，并提供“重试恢复”和“打开日志目录”。

## 7. 取消与窗口关闭

`MainWindowViewModel` 为每次操作创建独立 `CancellationTokenSource`。UI 状态至少包括：

- `Idle`
- `Preparing`
- `Running`
- `CancellationRequested`
- `RollingBack`
- `Succeeded`
- `Failed`
- `RecoveryFailed`

取消按钮仅请求取消，不关闭进程。收到请求后：

1. 主操作在安全检查点停止。
2. UI 显示“正在恢复之前状态”。
3. 引擎使用恢复 token 回滚。
4. 回滚结束后才允许关闭。

窗口 `Closing` 事件在状态不稳定时取消本次关闭，并调用 `RequestCloseAsync`：

- Idle/终态直接关闭。
- Running 时显示确认对话框；确认后请求安全取消。
- RollingBack 时拒绝关闭并显示当前恢复状态。
- RecoveryFailed 时允许用户打开日志，但默认不静默退出。

静默模式收到取消信号时遵循相同引擎语义，并使用非零退出码区分取消、执行失败和恢复失败。

## 8. 卸载路径保护

新增 `InstalledStateValidator`。任何删除动作之前必须：

1. 校验状态 `ProductId` 与当前产品一致。
2. 校验安装根目录中的 ownership marker 与状态一致。
3. 重新计算主程序、维护目录、状态目录和恢复目录的允许路径，不直接信任状态文件。
4. 对数据目录按照当前产品清单和 scope 重新解析；状态中的路径必须与计算结果相同。
5. 拒绝任何根目录、特殊目录本身、越界路径、`..` 逃逸或不受信重解析点。

如果归属或路径验证失败，卸载必须安全失败，不执行“尽力删除”。

## 9. 更新可信链

### 9.1 传输策略

- 本地文件路径继续支持离线安装。
- 远程 feed、包清单和包只允许 HTTPS。
- HTTP、FTP 和其他 scheme 在下载前直接拒绝。

### 9.2 签名策略

在线更新使用分离签名：

- 算法固定为 RSA-PSS + SHA-256。
- `latest.json.sig` 签名 `latest.json` 的原始字节。
- `package.manifest.json.sig` 签名包清单原始字节。
- 已签名 feed 内仍保存包 SHA-256；运行时同时验证 feed 签名、包清单签名、包哈希和逐文件哈希。
- 公钥通过产品清单中的相对路径引用，路径必须位于产品清单目录内。
- 私钥只由 Builder 命令参数或安全环境提供，绝不复制进模板、产物或日志。

`UpdateSettingsManifest` 新增：

- `RequireHttps`，默认 `true`
- `RequireSignature`，默认在启用在线更新时为 `true`
- `TrustedPublicKeyPath`

当 `AllowOnlineUpdate=true` 时，产品校验器要求启用 HTTPS、签名并提供可信公钥。现有关闭在线更新的产品清单保持兼容。

`publish-update-feed` 增加 `--signing-key <private.pem>`。启用在线更新而缺少签名密钥时，Builder 失败，不生成不安全 feed。

## 10. WPF Fluent Workbench

### 10.1 外壳

`MainWindow` 保留自定义窗口能力，但只负责：

- TitleBar 与窗口命令
- 左侧步骤 rail
- 当前页面 `ContentControl`
- 全局操作状态、取消和关闭协调

页面拆分为独立 UserControl：

- `WelcomePage`
- `LicensePage`
- `InstallOptionsPage`
- `ReviewPage`
- `ProgressPage`
- `RecoveryPage`
- `CompletionPage`
- `MaintenancePage`
- `UninstallConfirmationPage`

不再把所有页面放在一个 XAML 文件中用 Visibility 堆叠。

### 10.2 ViewModel 边界

保留 `MainWindowViewModel` 作为 shell 编排器，并拆出：

- `InstallOptionsViewModel`
- `OperationProgressViewModel`
- `MaintenanceViewModel`
- `RecoveryViewModel`

命令使用可等待、可禁用重入的异步命令。任何业务异常都转为结构化结果和 UI 状态，不从 `async void` 逃逸。

### 10.3 交互标准

- Rail 明确显示已完成、当前和未开始步骤。
- 主操作只有一个高强调按钮。
- 进度页显示总体百分比、当前步骤、已完成步骤和可展开日志。
- Cancel 显示安全含义；请求后变为“正在取消”。
- Recovery 页面与普通错误页分离。
- 成功页显示安装版本、安装位置和日志入口。
- 页面切换使用 140–180ms 的淡入与轻微位移动画；系统关闭动画时禁用。
- 支持高 DPI、键盘导航、焦点可见、屏幕阅读器名称及中英文资源。
- `Esc` 不直接杀进程；运行中等价于请求安全取消。

## 11. 错误模型与日志

扩展 `SetupOperationResult`：

- `Status`：Succeeded、Cancelled、Failed、RecoveryFailed
- `FailureCode`
- `PrimaryError`
- `RecoveryErrors`
- `OperationId`
- `LogPath`
- `RecoveryDirectory`

现有 `Succeeded` 属性保留为兼容入口，其值由 `Status == Succeeded` 推导；现有调用方不需要在同一提交中全部改写。

日志每行包含时间、OperationId、步骤、阶段和消息。路径、URL、异常和哈希可以记录；私钥、签名密钥内容和敏感命令参数不得记录。

UI 面向用户显示稳定错误代码和可行动建议；完整堆栈仅写日志。

## 12. 兼容性与迁移

- 现有离线产品清单继续可用。
- 现有 `SetupOperationResult.Succeeded` 调用保持源码兼容。
- `allowOverwrite` 保留字段但收紧语义，只允许已归属安装。
- 启用在线更新的旧清单会在验证时得到明确错误，要求补充 HTTPS 和签名配置。
- 旧安装没有 ownership marker 时，不自动覆盖。维护模式提供一次性“认领旧安装”迁移：只有外部状态 ProductId、主程序路径、安装根和版本均验证通过时才写入 marker。
- 旧 PendingBackupDirectory 只在其位于已知临时/恢复根时处理；越界路径忽略并报告，不删除。

## 13. 测试策略

### 13.1 Core 单元与集成测试

新增测试覆盖：

- 空目录首次安装成功。
- 非空无归属目录被拒绝。
- 同产品、同 InstallationId 的更新成功。
- 驱动器根、Windows、特殊目录本身和越界路径被拒绝。
- 用户在每个可变更步骤取消后都恢复原状态。
- 步骤执行中途异常时执行本步骤补偿。
- 回滚使用独立 token；原 token 已取消时仍恢复。
- 回滚失败保留 journal 和快照。
- 重启后检测并恢复未完成事务。
- 篡改状态文件不能删除允许根之外的目录。
- HTTP 更新被拒绝。
- feed、包清单、包或单文件任一被篡改时验证失败。
- 有效 RSA-PSS 签名完成更新。

所有文件系统测试使用唯一临时根和 fake system paths，绝不触碰真实 Program Files、注册表或用户目录。

### 13.2 WPF/ViewModel 测试

测试页面状态流、命令重入、取消状态、关闭守卫、恢复状态和中英文资源键。视图层保留少量人工 smoke test：高 DPI、键盘、动画开关、长文本和窗口缩放。

### 13.3 构建门禁

CI 至少执行：

1. `dotnet build RS.SetupApp/RS.SetupApp.csproj`
2. `dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj`
3. Builder 端到端生成包、签名 feed、验证并执行临时目录安装/升级/卸载测试

## 14. 实施顺序

1. 安装目标策略、ownership marker 与恶意状态路径测试。
2. 持久 journal、独立恢复 token、幂等回滚和崩溃恢复。
3. 取消/关闭编排及结构化结果模型。
4. Fluent Workbench XAML 外壳、页面拆分、状态动画与本地化。
5. HTTPS 策略、RSA-PSS 签名、Builder 参数和篡改测试。
6. 文档、模板迁移说明、完整构建和回归验证。

## 15. 验收标准

以下条件全部满足才视为完成：

- 安装到非空无归属目录时，在任何文件移动或删除前失败。
- 任意步骤失败或用户取消后，原安装内容逐文件保持一致。
- 恢复失败时快照和 journal 保留，下一次启动能够重试。
- 运行中关闭窗口不会直接终止安装进程。
- 卸载无法删除产品允许根之外的任何路径。
- 在线更新拒绝 HTTP、无签名和签名不匹配内容。
- WPF 页面与 Idle、Running、CancellationRequested、RollingBack、Succeeded、Failed、RecoveryFailed 状态一一对应。
- 安装器 UI 支持中英文、键盘、高 DPI 和系统动画设置。
- 新增安全测试与现有 16 项安装器测试全部通过。
- `RS.SetupApp`、`RS.SetupApp.Builder` 和 `RS.SetupApp.Tests` 构建无错误。
