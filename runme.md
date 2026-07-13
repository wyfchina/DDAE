# DDAE 启动服务指令

本文档记录 DDAE 当前阶段的本地启动流程。后续随着数据库、生产数据源、后台任务、端口策略或部署方式变化，本文件需要持续更新。

## 1. 基本信息

| 项目 | 当前值 |
|---|---|
| 开发目录 | `D:\Documents\DDAE` |
| Web 项目 | `src\AdaptiveSopDdsop.Web` |
| 测试项目 | `tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj` |
| 默认地址 | `http://127.0.0.1:5188` |
| 备用地址 | `http://127.0.0.1:5190` |
| .NET 命令 | `"$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe"` |

## 2. 启动前验证

| 顺序 | 指令 | 目的 |
|---:|---|---|
| 1 | `cd D:\Documents\DDAE` | 进入 DDAE 开发目录 |
| 2 | `& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" build AdaptiveSopDdsop.sln` | 构建解决方案，确认代码可编译 |
| 3 | `& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj` | 运行测试，确认核心业务逻辑通过 |

## 3. 启动 Web 服务

| 顺序 | 指令 | 目的 |
|---:|---|---|
| 1 | `cd D:\Documents\DDAE` | 确保从项目根目录启动 |
| 2 | `& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project src\AdaptiveSopDdsop.Web --urls http://127.0.0.1:5188` | 启动 Web 服务 |
| 3 | 打开 `http://127.0.0.1:5188` | 查看 Scenario Run Workspace 页面 |

如果 `5188` 端口被占用，使用备用端口：

```powershell
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project src\AdaptiveSopDdsop.Web --urls http://127.0.0.1:5190
```

然后打开：

```text
http://127.0.0.1:5190
```

## 4. 后台启动方式

如果需要让服务在后台运行，可以使用：

```powershell
cd D:\Documents\DDAE

$dotnet = "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe"
$out = "D:\Documents\DDAE\adaptive-sop-ddsop.5188.out.log"
$err = "D:\Documents\DDAE\adaptive-sop-ddsop.5188.err.log"

Start-Process -WindowStyle Hidden `
  -FilePath $dotnet `
  -ArgumentList @("run","--project","src\AdaptiveSopDdsop.Web","--urls","http://127.0.0.1:5188") `
  -WorkingDirectory "D:\Documents\DDAE" `
  -RedirectStandardOutput $out `
  -RedirectStandardError $err
```

后台启动后验证：

```powershell
Invoke-WebRequest -UseBasicParsing -Uri http://127.0.0.1:5188 -TimeoutSec 10
```

## 5. 查看日志

| 指令 | 目的 |
|---|---|
| `Get-Content -Path D:\Documents\DDAE\adaptive-sop-ddsop.5188.out.log -Tail 80` | 查看服务正常输出 |
| `Get-Content -Path D:\Documents\DDAE\adaptive-sop-ddsop.5188.err.log -Tail 120` | 查看服务错误输出 |

## 6. 停止服务

先查找监听端口对应进程：

```powershell
Get-NetTCPConnection -LocalAddress 127.0.0.1 -LocalPort 5188 -State Listen
```

然后停止对应进程：

```powershell
Stop-Process -Id <OwningProcess> -Force
```

## 7. 启动网络结构评分工作台

网络结构评分已经拆成独立产品线，目录不在 `D:\Documents\DDAE`，而是在：

```text
D:\Documents\DDAE-NetworkStructure
```

DDS&OP 主页面里的“打开网络结构评分工作台”默认跳转到：

```text
http://127.0.0.1:5296/network-structure
```

### 7.1 前台启动

打开一个新的 PowerShell 窗口，执行：

```powershell
cd D:\Documents\DDAE-NetworkStructure

& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run `
  --project src\AdaptiveSopDdsop.NetworkStructure.Host\AdaptiveSopDdsop.NetworkStructure.Host.csproj `
  --urls http://127.0.0.1:5296
```

看到类似下面的信息，表示服务已经启动：

```text
Now listening on: http://127.0.0.1:5296
Application started. Press Ctrl+C to shut down.
```

然后打开：

```text
http://127.0.0.1:5296/network-structure
```

### 7.2 后台启动

如果需要让网络结构评分在后台运行，可以执行：

```powershell
cd D:\Documents\DDAE-NetworkStructure

$dotnet = "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe"
$out = "D:\Documents\DDAE-NetworkStructure\network-host.5296.out.log"
$err = "D:\Documents\DDAE-NetworkStructure\network-host.5296.err.log"

Start-Process -WindowStyle Hidden `
  -FilePath $dotnet `
  -ArgumentList @("run","--project","src\AdaptiveSopDdsop.NetworkStructure.Host\AdaptiveSopDdsop.NetworkStructure.Host.csproj","--urls","http://127.0.0.1:5296") `
  -WorkingDirectory "D:\Documents\DDAE-NetworkStructure" `
  -RedirectStandardOutput $out `
  -RedirectStandardError $err
```

后台启动后验证：

```powershell
Invoke-WebRequest -UseBasicParsing -Uri http://127.0.0.1:5296/network-structure -TimeoutSec 10
```

如果返回 `StatusCode : 200`，说明网络结构评分工作台已正常启动。

### 7.3 从 DDS&OP 主页面进入

如果 DDS&OP 主服务也已启动：

```text
http://127.0.0.1:5188
```

可以在 DDS&OP 首页点击：

```text
打开网络结构评分工作台
```

该按钮会跳转到：

```text
http://127.0.0.1:5296/network-structure
```

### 7.4 停止网络结构评分服务

先查找监听 `5296` 端口的进程：

```powershell
Get-NetTCPConnection -LocalAddress 127.0.0.1 -LocalPort 5296 -State Listen
```

然后停止对应进程：

```powershell
Stop-Process -Id <OwningProcess> -Force
```

### 7.5 网络结构评分与 DDS&OP 的关系

| 项目 | 说明 |
|---|---|
| DDS&OP 主服务 | `D:\Documents\DDAE`，默认端口 `5188` |
| 网络结构评分服务 | `D:\Documents\DDAE-NetworkStructure`，默认端口 `5296` |
| 页面入口 | DDS&OP 首页的“打开网络结构评分工作台”按钮 |
| 数据边界 | 网络结构评分输出候选控制点、缓冲点、证据链和不采纳风险；候选建议不自动采纳 |
| 回算边界 | 候选动作进入 DDS&OP 后，仍必须由 DDS&OP 白盒场景重新计算 |

## 8. 后续升级记录

| 日期 | 更新内容 | 状态 |
|---|---|---|
| 2026-06-23 | 新建本地启动、后台启动、日志查看、停止服务说明。 | Done |
| 2026-07-02 | 补充网络结构评分独立产品线启动、后台启动、入口和停止服务说明。 | Done |
