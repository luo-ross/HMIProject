[CmdletBinding()]
param(
    [switch]$KeepArtifacts,
    [string]$ArtifactRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$testArtifactRoot = $ArtifactRoot
if ([string]::IsNullOrWhiteSpace($testArtifactRoot)) {
    $testArtifactRoot = Join-Path $repoRoot "artifacts\setupapp-test\silent-$([Guid]::NewGuid().ToString('N'))"
}

. (Join-Path $PSScriptRoot 'Setup-SetupAppFixture.ps1')
$fixture = $null
$failed = $true

function Invoke-FixtureSetup {
    param(
        [Parameter(Mandatory)] [string]$SetupPath,
        [Parameter(Mandatory)] [string]$Mode,
        [Parameter(Mandatory)] [string]$LogName,
        [switch]$UseExternalFixtureManifest,
        [int]$ExpectedExitCode = 0
    )

    $logPath = Join-Path $fixture.ArtifactRoot "logs\$LogName.jsonl"
    # Repair/update/uninstall normally dispatch to a copied worker and the parent exits after launch.
    # The gate must wait for the actual generated Setup.exe operation and its documented exit code.
    $arguments = @('--mode', $Mode, '--silent', '--worker', '--scope', 'user', '--install-dir', $fixture.InstallRoot, '--skip-launch', '--no-shortcuts', '--no-autostart', '--purge-data', '--log', $logPath)
    if ($UseExternalFixtureManifest) {
        $arguments += @('--product', $fixture.ProductManifestPath)
    }

    $argumentLine = ($arguments | ForEach-Object { '"' + $_.Replace('"', '\"') + '"' }) -join ' '
    $process = Start-Process -FilePath $SetupPath -ArgumentList $argumentLine -PassThru -Wait
    if ($process.ExitCode -ne $ExpectedExitCode) {
        throw "Setup.exe $Mode returned $($process.ExitCode); expected $ExpectedExitCode. See '$logPath'."
    }
}

function Remove-FixtureState {
    if ($null -eq $fixture) { return }
    foreach ($path in @($fixture.InstallRoot, $fixture.StateRoot, $fixture.RecoveryRoot, $fixture.DataRoot, $fixture.LogRoot)) {
        if (Test-Path -LiteralPath $path) {
            Remove-Item -LiteralPath $path -Recurse -Force
        }
    }

    if (Test-Path -LiteralPath $fixture.RegistryPath) {
        Remove-Item -LiteralPath $fixture.RegistryPath -Recurse -Force
    }
}

try {
    $fixture = New-SetupAppLifecycleFixture -RepositoryRoot $repoRoot -ArtifactRoot $testArtifactRoot
    if ($fixture.InstallRoot -match '(?i)program files|programdata') {
        throw "Fixture install root must remain disposable: '$($fixture.InstallRoot)'."
    }

    # Regression: a supplied manifest outside the bundled payload has no package, and Setup.exe must
    # propagate the core failure as exit code 3 instead of silently returning success.
    Invoke-FixtureSetup -SetupPath $fixture.V1.SetupPath -Mode 'install' -LogName 'missing-payload-exit-code' -UseExternalFixtureManifest -ExpectedExitCode 3
    if (-not (Select-String -LiteralPath (Join-Path $fixture.ArtifactRoot 'logs\missing-payload-exit-code.jsonl') -Pattern 'No offline package was found' -Quiet)) {
        throw 'The missing-payload regression did not reach the expected core failure path.'
    }

    Invoke-FixtureSetup -SetupPath $fixture.V1.SetupPath -Mode 'install' -LogName 'install-v1'
    if (-not (Test-Path -LiteralPath (Join-Path $fixture.InstallRoot 'TestPayloadApp.exe'))) { throw 'v1 install did not deploy the payload.' }
    if (-not (Test-Path -LiteralPath $fixture.RegistryPath)) { throw 'v1 install did not create its generated HKCU uninstall entry.' }
    $v1Hash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $fixture.InstallRoot 'TestPayloadApp.exe')).Hash

    Set-Content -LiteralPath (Join-Path $fixture.InstallRoot 'fixture-version.txt') -Value 'tampered' -NoNewline
    Invoke-FixtureSetup -SetupPath $fixture.V1.SetupPath -Mode 'repair' -LogName 'repair-v1'
    if ((Get-Content -Raw -LiteralPath (Join-Path $fixture.InstallRoot 'fixture-version.txt')) -ne '1.0.0') { throw 'Repair did not restore v1 payload content.' }

    # Cancellation rollback is deterministic in SetupLifecycleTests. The bundled UI suite exercises the visible cancel action.
    & dotnet test (Join-Path $repoRoot 'RS.SetupApp.Tests\RS.SetupApp.Tests.csproj') --filter 'FullyQualifiedName~SetupLifecycleTests' --no-restore *> (Join-Path $fixture.ArtifactRoot 'logs\core-lifecycle.log')
    if ($LASTEXITCODE -ne 0) { throw 'Core cancellation/rollback regression test failed.' }

    Invoke-FixtureSetup -SetupPath $fixture.V2.SetupPath -Mode 'update' -LogName 'update-v2'
    $v2Hash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $fixture.InstallRoot 'TestPayloadApp.exe')).Hash
    if ($v1Hash -eq $v2Hash) { throw 'The generated v2 payload did not replace the v1 executable bytes.' }
    if ((Get-Content -Raw -LiteralPath (Join-Path $fixture.InstallRoot 'fixture-version.txt')) -ne '2.0.0') { throw 'v2 update did not deploy the v2 payload.' }

    $statePath = Join-Path $fixture.StateRoot 'installed-state.json'
    $validState = Get-Content -Raw -LiteralPath $statePath
    $hostileState = $validState | ConvertFrom-Json
    $hostileState.productId = "hostile-$([Guid]::NewGuid().ToString('N'))"
    $hostileState | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $statePath -Encoding utf8
    $sentinelPath = Join-Path $fixture.ArtifactRoot 'sentinel.txt'
    Set-Content -LiteralPath $sentinelPath -Value 'do not modify' -NoNewline
    Invoke-FixtureSetup -SetupPath $fixture.V2.SetupPath -Mode 'uninstall' -LogName 'hostile-uninstall' -ExpectedExitCode 3
    if ((Get-Content -Raw -LiteralPath $sentinelPath) -ne 'do not modify') { throw 'Hostile uninstall modified the external sentinel.' }

    Set-Content -LiteralPath $statePath -Value $validState -Encoding utf8
    Invoke-FixtureSetup -SetupPath $fixture.V2.SetupPath -Mode 'uninstall' -LogName 'uninstall-v2'
    foreach ($path in @($fixture.InstallRoot, $fixture.StateRoot, $fixture.RecoveryRoot, $fixture.DataRoot)) {
        if (Test-Path -LiteralPath $path) { throw "Uninstall left generated fixture state at '$path'." }
    }
    if (Test-Path -LiteralPath $fixture.RegistryPath) { throw 'Uninstall left the generated HKCU uninstall entry.' }

    $failed = $false
    Write-Host 'SetupApp silent lifecycle passed.'
}
finally {
    Remove-FixtureState
    if (-not $failed -and -not $KeepArtifacts -and (Test-Path -LiteralPath $testArtifactRoot)) {
        Remove-Item -LiteralPath $testArtifactRoot -Recurse -Force
    }
    elseif ($failed -or $KeepArtifacts) {
        Write-Host "SetupApp artifacts retained at '$testArtifactRoot'."
    }
}
