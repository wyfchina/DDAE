# DDAE 本地启动与检查记录

本文档是 DDAE 在 Windows 新电脑上的标准启动入口。命令使用 `$HOME\Documents`，不依赖旧设备盘符；DDAE、NetworkStructure 和接口契约仍按独立目录维护。

## 1. 当前标准

| 项目 | 当前值 |
|---|---|
| DDAE 根目录 | `$HOME\Documents\DDAE` |
| 当前设备目录 | `C:\Users\吴一帆\Documents\DDAE` |
| Web 项目 | `src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj` |
| 测试项目 | `tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj` |
| 标准检查地址 | `http://127.0.0.1:5188` |
| 备用检查地址 | `http://127.0.0.1:5190` |
| SDK | .NET 9（由仓库 `global.json` 约束） |
| 正式开发分支 | `main` |

首次迁移到新电脑时，三个目录建议保持为同级目录：

```text
$HOME\Documents\DDAE
$HOME\Documents\DDAE-NetworkStructure
$HOME\Documents\DDAE_INTERFACE_CONTRACT
```

DDAE 主服务不要求 NetworkStructure 同时运行，也不会在启动时修改接口契约仓库。

## 2. 启动前验证

打开 PowerShell，执行：

```powershell
$root = Join-Path $HOME "Documents\DDAE"
Set-Location -LiteralPath $root

git status --short --branch
dotnet --version
dotnet build .\AdaptiveSopDdsop.sln -c Release --no-restore -m:1
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -c Release --no-build
```

新电脑第一次构建、尚未生成依赖缓存时，先执行：

```powershell
dotnet restore .\AdaptiveSopDdsop.sln
```

当前验收基线：

- 测试：`259 test(s) passed.`
- Release 构建：`0 个警告，0 个错误`

## 3. 前台启动 DDAE

```powershell
$root = Join-Path $HOME "Documents\DDAE"
Set-Location -LiteralPath $root

dotnet run `
  --project .\src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj `
  -c Release `
  --no-build `
  --no-launch-profile `
  -- `
  --urls http://127.0.0.1:5188 `
  --environment Development
```

本地从源码构建目录运行时必须使用 `Development` 环境，以启用 ASP.NET Core 的静态 Web 资源清单；否则浏览器请求压缩版 CSS/JavaScript 时可能得到空响应。Production 环境应运行 `dotnet publish` 的发布目录，不直接运行源码构建目录。

看到以下信息后即可检查：

```text
Now listening on: http://127.0.0.1:5188
Application started. Press Ctrl+C to shut down.
```

浏览器地址：

```text
http://127.0.0.1:5188
```

如果 `5188` 已被占用，把启动命令和后续检查命令中的端口统一改为 `5190`。

## 4. 后台启动 DDAE

需要保留服务供浏览器检查时，使用隐藏后台进程：

```powershell
$root = Join-Path $HOME "Documents\DDAE"
$logRoot = Join-Path $root ".logs"
$out = Join-Path $logRoot "ddae.5188.out.log"
$err = Join-Path $logRoot "ddae.5188.err.log"

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null

$process = Start-Process -WindowStyle Hidden `
  -FilePath "dotnet" `
  -ArgumentList @(
    "run",
    "--project", ".\src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj",
    "-c", "Release",
    "--no-build",
    "--no-launch-profile",
    "--",
    "--urls", "http://127.0.0.1:5188",
    "--environment", "Development"
  ) `
  -WorkingDirectory $root `
  -RedirectStandardOutput $out `
  -RedirectStandardError $err `
  -PassThru

$process.Id
```

`.logs` 和所有 `*.log` 已由 `.gitignore` 排除，不会进入 Git。

## 5. 健康检查

```powershell
Invoke-WebRequest `
  -UseBasicParsing `
  -Uri http://127.0.0.1:5188/ `
  -TimeoutSec 10

Invoke-RestMethod `
  -Uri "http://127.0.0.1:5188/api/history-review?trendMonths=6" `
  -TimeoutSec 10
```

首页返回 HTTP `200`，历史回顾接口返回 JSON，即表示当前 DDAE 主服务可供检查。

浏览器首次检查还应确认页面为深绿侧栏布局，而不是浏览器默认的裸文本样式；裸文本表示 CSS/JavaScript 静态资源未正确加载。

查看日志：

```powershell
$root = Join-Path $HOME "Documents\DDAE"
Get-Content -Path (Join-Path $root ".logs\ddae.5188.out.log") -Tail 80
Get-Content -Path (Join-Path $root ".logs\ddae.5188.err.log") -Tail 120
```

## 6. 停止 DDAE

前台启动时按 `Ctrl+C`。后台启动时按监听端口停止实际服务进程：

```powershell
$connection = Get-NetTCPConnection `
  -LocalAddress 127.0.0.1 `
  -LocalPort 5188 `
  -State Listen `
  -ErrorAction SilentlyContinue `
  | Select-Object -First 1

if ($connection) {
  Stop-Process -Id $connection.OwningProcess
}
```

## 7. NetworkStructure 独立启动

NetworkStructure 保持独立 worktree/产品线，不并入 DDAE `main`。只有检查网络结构评分工作台时才需要单独启动：

```powershell
$networkRoot = Join-Path $HOME "Documents\DDAE-NetworkStructure"
Set-Location -LiteralPath $networkRoot

dotnet run `
  --project .\src\AdaptiveSopDdsop.NetworkStructure.Host\AdaptiveSopDdsop.NetworkStructure.Host.csproj `
  --urls http://127.0.0.1:5296
```

检查地址：

```text
http://127.0.0.1:5296/network-structure
```

NetworkStructure 的候选结果仍只是治理建议，不会自动采纳，也不会绕过 DDAE 白盒场景重算。

## 8. 实际启动记录

| 日期 | 分支与提交 | 目录 | 地址 | 启动方式 | 进程 | 健康检查 |
|---|---|---|---|---|---|---|
| 2026-07-19 06:37 +08:00 | `main` / `9dfdf01`（应用源码） | `C:\Users\吴一帆\Documents\DDAE` | `http://127.0.0.1:5188` | Release 构建、Development 源码运行 | `12352`（`AdaptiveSopDdsop.Web`） | 首页 `200`；历史回顾接口 `200`；压缩 CSS/JavaScript 正常；浏览器样式与数据加载正常 |

## 9. 更新记录

| 日期 | 更新内容 | 状态 |
|---|---|---|
| 2026-06-23 | 新建本地启动、后台启动、日志查看和停止服务说明。 | Done |
| 2026-07-02 | 补充 NetworkStructure 独立产品线启动与边界说明。 | Done |
| 2026-07-19 | 移除旧设备盘符依赖，统一 `$HOME\Documents`、Release 构建、Development 源码启动、静态资源检查和实际启动记录。 | Done |
