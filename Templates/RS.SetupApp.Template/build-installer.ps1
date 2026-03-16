$ErrorActionPreference = "Stop"

$productManifest = Join-Path $PSScriptRoot "product.json"
$publishDirectory = Join-Path $PSScriptRoot "publish"
$packageOutput = Join-Path $PSScriptRoot "artifacts\packages"
$installerOutput = Join-Path $PSScriptRoot "artifacts\installer"

# 依次生成更新包、更新清单和离线安装器。
dotnet run --project ..\..\RS.SetupApp.Builder -- pack --from-dir $publishDirectory --product $productManifest --output $packageOutput
dotnet run --project ..\..\RS.SetupApp.Builder -- publish-update-feed --package $packageOutput --base-url https://example.com/downloads/
dotnet run --project ..\..\RS.SetupApp.Builder -- build-installer --product $productManifest --package $packageOutput --output $installerOutput
