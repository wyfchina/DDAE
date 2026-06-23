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

## 7. 后续升级记录

| 日期 | 更新内容 | 状态 |
|---|---|---|
| 2026-06-23 | 新建本地启动、后台启动、日志查看、停止服务说明。 | Done |
