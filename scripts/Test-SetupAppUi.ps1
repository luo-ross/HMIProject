[CmdletBinding()]
param(
    [switch]$KeepArtifacts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$previousKeepArtifacts = $env:RS_SETUPAPP_KEEP_ARTIFACTS
try {
    if ($KeepArtifacts) {
        $env:RS_SETUPAPP_KEEP_ARTIFACTS = '1'
    }
    else {
        Remove-Item Env:RS_SETUPAPP_KEEP_ARTIFACTS -ErrorAction SilentlyContinue
    }

    $testOutput = & dotnet test (Join-Path $repositoryRoot 'RS.SetupApp.AutomationTests\RS.SetupApp.AutomationTests.csproj') --filter 'FullyQualifiedName~SetupAppUiLifecycleTests' 2>&1
    $testOutput | Write-Output
    if ($LASTEXITCODE -ne 0) {
        throw "FlaUI lifecycle tests failed with exit code $LASTEXITCODE. Generated screenshots and logs were retained under artifacts/setupapp-test."
    }

    if ($testOutput -match 'SetupAppFlaUIInteractiveDesktopRequired') {
        $message = 'FlaUI lifecycle is inconclusive because this session has no capturable interactive desktop; artifacts/setupapp-test retains the black-frame evidence. Set RS_SETUPAPP_REQUIRE_INTERACTIVE_UI=1 on an interactive Windows runner to make this condition fail the gate.'
        if ($env:RS_SETUPAPP_REQUIRE_INTERACTIVE_UI -eq '1') {
            throw $message
        }

        Write-Warning $message
    }
}
finally {
    if ($null -eq $previousKeepArtifacts) {
        Remove-Item Env:RS_SETUPAPP_KEEP_ARTIFACTS -ErrorAction SilentlyContinue
    }
    else {
        $env:RS_SETUPAPP_KEEP_ARTIFACTS = $previousKeepArtifacts
    }
}
