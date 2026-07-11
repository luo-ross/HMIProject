[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [string]$ArtifactRoot,
    [switch]$EmitFixtureJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-SetupAppArtifactRoot {
    param(
        [Parameter(Mandatory)] [string]$Root,
        [Parameter(Mandatory)] [string]$RepositoryRoot
    )

    $allowedRoot = [IO.Path]::GetFullPath((Join-Path $RepositoryRoot 'artifacts\setupapp-test'))
    $fullRoot = [IO.Path]::GetFullPath($Root)
    $prefix = $allowedRoot.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $fullRoot.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "ArtifactRoot must be a generated child of '$allowedRoot'."
    }

    return $fullRoot
}

function Invoke-SetupAppDotnet {
    param(
        [Parameter(Mandatory)] [string]$ArtifactRoot,
        [Parameter(Mandatory)] [string]$Name,
        [Parameter(Mandatory)] [string[]]$Arguments
    )

    $logPath = Join-Path $ArtifactRoot "logs\$Name.log"
    & dotnet @Arguments *> $logPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $Name failed with exit code $LASTEXITCODE. See '$logPath'."
    }
}

function Set-SetupAppPackageVersion {
    param(
        [Parameter(Mandatory)] [string]$PackageDirectory,
        [Parameter(Mandatory)] [string]$Version
    )

    $manifestPath = Join-Path $PackageDirectory 'package.manifest.json'
    $package = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
    $package.version = $Version
    $package | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $manifestPath -Encoding utf8
}

function New-SetupAppLifecycleFixture {
    param(
        [Parameter(Mandatory)] [string]$RepositoryRoot,
        [Parameter(Mandatory)] [string]$ArtifactRoot
    )

    $RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
    $ArtifactRoot = Assert-SetupAppArtifactRoot -Root $ArtifactRoot -RepositoryRoot $RepositoryRoot
    if (Test-Path -LiteralPath $ArtifactRoot) {
        throw "Fixture artifact root '$ArtifactRoot' already exists."
    }

    New-Item -ItemType Directory -Path (Join-Path $ArtifactRoot 'logs') -Force | Out-Null
    $productId = "setup-lifecycle-$([Guid]::NewGuid().ToString('N'))"
    $fixtureSource = Join-Path $RepositoryRoot 'RS.SetupApp.Tests\Fixtures\TestPayloadApp'
    $fixtureInput = Join-Path $ArtifactRoot 'fixture'
    $installRoot = Join-Path $ArtifactRoot 'install'
    $packagesRoot = Join-Path $ArtifactRoot 'packages'
    $bundlesRoot = Join-Path $ArtifactRoot 'bundles'
    New-Item -ItemType Directory -Path $fixtureInput, $packagesRoot, $bundlesRoot -Force | Out-Null

    Copy-Item -LiteralPath (Join-Path $fixtureSource 'fixture.product.json') -Destination (Join-Path $fixtureInput 'product.json')
    Copy-Item -LiteralPath (Join-Path $fixtureSource 'product.schema.json') -Destination (Join-Path $fixtureInput 'product.schema.json')
    Copy-Item -LiteralPath (Join-Path $fixtureSource 'LICENSE.txt') -Destination (Join-Path $fixtureInput 'LICENSE.txt')

    $productManifestPath = Join-Path $fixtureInput 'product.json'
    $product = Get-Content -Raw -LiteralPath $productManifestPath | ConvertFrom-Json
    $product.productId = $productId
    $product.displayName = "RS.SetupApp Lifecycle $($productId.Substring($productId.Length - 8))"
    $product.installDefaults.defaultInstallDirectoryOverride = $installRoot
    $product.dataDirectories[0].relativePath = "RS.SetupApp-Fixtures\$productId"
    $product | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $productManifestPath -Encoding utf8

    $payloadProject = Join-Path $fixtureSource 'TestPayloadApp.csproj'
    $builderProject = Join-Path $RepositoryRoot 'RS.SetupApp.Builder\RS.SetupApp.Builder.csproj'
    $runtimeProject = Join-Path $RepositoryRoot 'RS.SetupApp\RS.SetupApp.csproj'
    $versions = @(
        [pscustomobject]@{ Version = '1.0.0'; Name = 'v1' },
        [pscustomobject]@{ Version = '2.0.0'; Name = 'v2' }
    )
    $outputs = @{}
    foreach ($item in $versions) {
        $publishDirectory = Join-Path $ArtifactRoot "publish\$($item.Name)"
        $packageDirectory = Join-Path $packagesRoot $item.Name
        $bundleDirectory = Join-Path $bundlesRoot $item.Name
        Invoke-SetupAppDotnet -ArtifactRoot $ArtifactRoot -Name "publish-$($item.Name)" -Arguments @(
            'publish', $payloadProject, '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true',
            "-p:Version=$($item.Version)", "-p:AssemblyVersion=$($item.Version).0", "-p:FileVersion=$($item.Version).0",
            '-o', $publishDirectory
        )
        Set-Content -LiteralPath (Join-Path $publishDirectory 'fixture-version.txt') -Value $item.Version -NoNewline -Encoding utf8
        if ($item.Name -eq 'v2') {
            $randomBytes = [byte[]]::new(8MB)
            $randomGenerator = [Security.Cryptography.RandomNumberGenerator]::Create()
            try {
                $randomGenerator.GetBytes($randomBytes)
            }
            finally {
                $randomGenerator.Dispose()
            }
            [IO.File]::WriteAllBytes((Join-Path $publishDirectory 'update-cancel-window.bin'), $randomBytes)
        }

        Invoke-SetupAppDotnet -ArtifactRoot $ArtifactRoot -Name "pack-$($item.Name)" -Arguments @(
            'run', '--project', $builderProject, '-c', 'Release', '--', 'pack', '--from-dir', $publishDirectory,
            '--product', $productManifestPath, '--output', $packageDirectory
        )
        Set-SetupAppPackageVersion -PackageDirectory $packageDirectory -Version $item.Version
        Invoke-SetupAppDotnet -ArtifactRoot $ArtifactRoot -Name "bundle-$($item.Name)" -Arguments @(
            'run', '--project', $builderProject, '-c', 'Release', '--', 'build-installer', '--product', $productManifestPath,
            '--package', $packageDirectory, '--output', $bundleDirectory, '--runtime-project', $runtimeProject,
            '--configuration', 'Release', '--runtime', 'win-x64'
        )
        $setupPath = Join-Path $bundleDirectory 'Setup.exe'
        if (-not (Test-Path -LiteralPath $setupPath)) {
            throw "Bundled Setup.exe was not generated at '$setupPath'."
        }

        $outputs[$item.Name] = [pscustomobject]@{
            SetupPath = $setupPath
            PackageDirectory = $packageDirectory
            PublishDirectory = $publishDirectory
        }
    }

    $localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
    return [pscustomobject]@{
        ArtifactRoot = $ArtifactRoot
        ProductId = $productId
        ProductManifestPath = $productManifestPath
        InstallRoot = $installRoot
        StateRoot = Join-Path $localAppData "RS.SetupApp\InstalledProducts\$productId"
        RecoveryRoot = Join-Path $localAppData "RS.SetupApp\Recovery\$productId"
        LogRoot = Join-Path $localAppData "RS.SetupApp\Logs\$productId"
        DataRoot = Join-Path $localAppData "RS.SetupApp-Fixtures\$productId"
        RegistryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$productId"
        V1 = $outputs['v1']
        V2 = $outputs['v2']
    }
}

if ($EmitFixtureJson) {
    if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
        $RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
    }

    if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
        $ArtifactRoot = Join-Path $RepositoryRoot "artifacts\setupapp-test\fixture-$([Guid]::NewGuid().ToString('N'))"
    }

    New-SetupAppLifecycleFixture -RepositoryRoot $RepositoryRoot -ArtifactRoot $ArtifactRoot | ConvertTo-Json -Depth 8 -Compress
}
