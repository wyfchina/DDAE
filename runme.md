# DDAE 本地启动说明

本文档记录当前 DDAE 本地开发环境的干净启动流程。当前仓库包含两个可启动入口：

| 应用 | 用途 | 默认端口 | 默认地址 |
|---|---|---:|---|
| DDS&OP 主业务平台 | 场景运行、产品族看板、RCCP、供应商需求、主设置治理、契约接口 | 5188 | `http://127.0.0.1:5188` |
| 网络结构评分工作台 | 独立网络结构评分产品页，查看 BOM 网络、指标、证据链 | 5191 | `http://127.0.0.1:5191/network-structure` |

固定开发目录：

```powershell
cd "D:\Documents\DDAE"
```

.NET 命令：

```powershell
$dotnet = "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe"
```

## 1. 启动前验证

建议在启动前先确认项目能构建、测试能通过：

```powershell
cd "D:\Documents\DDAE"

& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" build AdaptiveSopDdsop.sln
```

如果 build 提示文件被占用，通常是 Web 服务还在运行，先按本文“停止服务”章节释放端口。

## 2. 启动 DDS&OP 主业务平台

前台启动，适合开发调试：

```powershell
cd "D:\Documents\DDAE"

& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run `
  --project src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj `
  --urls http://127.0.0.1:5188
```

看到类似下面内容表示启动成功：

```text
Now listening on: http://127.0.0.1:5188
Application started. Press Ctrl+C to shut down.
```

浏览器打开：

```text
http://127.0.0.1:5188
```

可选接口验证：

```powershell
Invoke-WebRequest -UseBasicParsing -Uri http://127.0.0.1:5188 -TimeoutSec 10
Invoke-RestMethod -Uri "http://127.0.0.1:5188/api/scenario-workspace-data?horizonWeeks=12"
```

说明：如果控制台出现 `Failed to determine the https port for redirect`，通常不影响本地 HTTP 访问。

## 3. 启动网络结构评分工作台

前台启动：

```powershell
cd "D:\Documents\DDAE"

& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run `
  --project src\AdaptiveSopDdsop.NetworkStructure.Host\AdaptiveSopDdsop.NetworkStructure.Host.csproj `
  --urls http://127.0.0.1:5191
```

浏览器打开：

```text
http://127.0.0.1:5191/network-structure
```

可选接口验证：

```powershell
Invoke-WebRequest -UseBasicParsing -Uri http://127.0.0.1:5191/network-structure -TimeoutSec 10
Invoke-RestMethod -Uri "http://127.0.0.1:5191/api/network-structure-data?horizonWeeks=12"
Invoke-RestMethod -Uri "http://127.0.0.1:5191/api/network-graph?itemCode=PART-FPGA-SPACE&maxDepth=6"
```

## 4. 同时启动两个应用

打开两个 PowerShell 窗口：

窗口 1 启动 DDS&OP 主业务平台：

```powershell
cd "D:\Documents\DDAE"

& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run `
  --project src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj `
  --urls http://127.0.0.1:5188
```

窗口 2 启动网络结构评分工作台：

```powershell
cd "D:\Documents\DDAE"

& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run `
  --project src\AdaptiveSopDdsop.NetworkStructure.Host\AdaptiveSopDdsop.NetworkStructure.Host.csproj `
  --urls http://127.0.0.1:5191
```

访问地址：

```text
DDS&OP 主业务平台: http://127.0.0.1:5188
网络结构评分工作台: http://127.0.0.1:5191/network-structure
```

## 5. 后台启动

后台启动 DDS&OP 主业务平台：

```powershell
cd "D:\Documents\DDAE"

$dotnet = "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe"

Start-Process -WindowStyle Hidden `
  -FilePath $dotnet `
  -ArgumentList @("run","--project","src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj","--urls","http://127.0.0.1:5188") `
  -WorkingDirectory "D:\Documents\DDAE" `
  -RedirectStandardOutput "D:\Documents\DDAE\ddae-web.out.log" `
  -RedirectStandardError "D:\Documents\DDAE\ddae-web.err.log"
```

后台启动网络结构评分工作台：

```powershell
cd "D:\Documents\DDAE"

$dotnet = "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe"

Start-Process -WindowStyle Hidden `
  -FilePath $dotnet `
  -ArgumentList @("run","--project","src\AdaptiveSopDdsop.NetworkStructure.Host\AdaptiveSopDdsop.NetworkStructure.Host.csproj","--urls","http://127.0.0.1:5191") `
  -WorkingDirectory "D:\Documents\DDAE" `
  -RedirectStandardOutput "D:\Documents\DDAE\network-host.out.log" `
  -RedirectStandardError "D:\Documents\DDAE\network-host.err.log"
```

后台启动后验证：

```powershell
Invoke-WebRequest -UseBasicParsing -Uri http://127.0.0.1:5188 -TimeoutSec 10
Invoke-WebRequest -UseBasicParsing -Uri http://127.0.0.1:5191/network-structure -TimeoutSec 10
```

## 6. 查看日志

```powershell
Get-Content -Path "D:\Documents\DDAE\ddae-web.out.log" -Tail 80
Get-Content -Path "D:\Documents\DDAE\ddae-web.err.log" -Tail 120

Get-Content -Path "D:\Documents\DDAE\network-host.out.log" -Tail 80
Get-Content -Path "D:\Documents\DDAE\network-host.err.log" -Tail 120
```

## 7. 停止服务

如果是前台启动，在对应 PowerShell 窗口按：

```text
Ctrl+C
```

如果需要按端口停止后台服务：

```powershell
$ports = @(5188, 5191)
foreach ($port in $ports) {
  $processIds = Get-NetTCPConnection `
    -LocalAddress 127.0.0.1 `
    -LocalPort $port `
    -State Listen `
    -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty OwningProcess -Unique

  foreach ($processId in $processIds) {
    Stop-Process -Id $processId -Force
  }
}
```

只停止 DDS&OP 主业务平台：

```powershell
$processIds = Get-NetTCPConnection -LocalAddress 127.0.0.1 -LocalPort 5188 -State Listen -ErrorAction SilentlyContinue |
  Select-Object -ExpandProperty OwningProcess -Unique

foreach ($processId in $processIds) {
  Stop-Process -Id $processId -Force
}
```

只停止网络结构评分工作台：

```powershell
$processIds = Get-NetTCPConnection -LocalAddress 127.0.0.1 -LocalPort 5191 -State Listen -ErrorAction SilentlyContinue |
  Select-Object -ExpandProperty OwningProcess -Unique

foreach ($processId in $processIds) {
  Stop-Process -Id $processId -Force
}
```

## 8. 常见问题

### 端口被占用

先按“停止服务”释放 `5188` 或 `5191`，再重新启动。

### 页面一直显示正在加载

优先检查浏览器控制台和后台日志。常见原因是前端调用的 API 报错，或服务启动的不是当前项目。

主业务平台至少验证：

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:5188/api/scenario-workspace-data?horizonWeeks=12"
```

网络结构评分至少验证：

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:5191/api/network-structure-data?horizonWeeks=12"
```

### build 时提示 DLL / EXE / cache 文件被占用

先停止正在运行的 Web 服务，再执行测试和 build。

### 浏览器打不开页面

确认访问的是本地 HTTP 地址：

```text
http://127.0.0.1:5188
http://127.0.0.1:5191/network-structure
```

不要使用旧的 `file:///.../index.html` 地址。

### GitHub push 连接失败

如果以后再次出现可以访问网页但 `git push` 连接 GitHub 失败的情况，先确认代理：

```powershell
git config --global https.proxy http://127.0.0.1:7890
```

push 完成后如需关闭 Git 代理：

```powershell
git config --global --unset https.proxy
```

## 9. 维护记录

| 日期 | 更新内容 |
|---|---|
| 2026-06-27 | 重写启动说明，区分 DDS&OP 主业务平台和网络结构评分工作台，补充后台启动、健康检查、停止服务和 GitHub 代理提醒。 |
