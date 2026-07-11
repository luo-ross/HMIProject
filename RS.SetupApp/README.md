# RS.SetupApp

`RS.SetupApp` is a reusable Windows installer stack for desktop applications.

## Projects

- `RS.SetupApp.Core`: setup engine, manifests, schema validation, rollback pipeline, and system integrations.
- `RS.SetupApp.Builder`: packaging CLI for offline bundles, update feeds, and branded installer output.
- `RS.SetupApp`: generic WPF runtime and maintenance wizard.
- `Templates/RS.SetupApp.Template`: starter template for new products.

## Typical workflow

1. Copy the template folder into your product repository.
2. Update `product.json`, icon, and license.
3. Publish your application or point the builder at your `.csproj`.
4. Run:

```powershell
dotnet run --project RS.SetupApp.Builder -- validate --product .\product.json
dotnet run --project RS.SetupApp.Builder -- pack --from-dir .\publish --product .\product.json
dotnet run --project RS.SetupApp.Builder -- publish-update-feed --package .\artifacts\packages\<product>\<version> --base-url https://example.com/downloads/
dotnet run --project RS.SetupApp.Builder -- build-installer --product .\product.json --package .\artifacts\packages\<product>\<version>
```

## Runtime arguments

- `Setup.exe --mode install|repair|update|uninstall`
- `--scope user|machine`
- `--product <product.json>`
- `--package <package.zip>`
- `--manifest <package.manifest.json>`
- `--update-manifest <latest.json or url>`
- `--install-dir <path>`
- `--purge-data`
- `--no-shortcuts`
- `--no-autostart`
- `--launch`
- `--skip-launch`
- `--silent`

## Release and lifecycle gates

Build a release bundle from a published payload, then run the disposable regression gates:

```powershell
dotnet restore MultiVerseKit.sln
dotnet build RS.SetupApp.Core/RS.SetupApp.Core.csproj -c Release
dotnet build RS.SetupApp.Builder/RS.SetupApp.Builder.csproj -c Release
dotnet build RS.SetupApp/RS.SetupApp.csproj -c Release
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~SetupLifecycleTests"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SetupAppEndToEnd.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SetupAppUi.ps1
```

The scripts generate a product id, payload, install root, and artifact root for every run. They use a bundled `Setup.exe`, never a development runtime. The generated install root is under `artifacts/setupapp-test`; current-user state, recovery, logs, and the exact generated HKCU uninstall key are removed in `finally`. Screenshots, logs, bundles, and packages are retained only on failure or when `-KeepArtifacts` / `RS_SETUPAPP_KEEP_ARTIFACTS=1` is specified.

The FlaUI test uses AutomationIds exclusively. It requires an interactive Windows desktop that can capture frames. A black or non-interactive desktop is reported as `SetupAppFlaUIInteractiveDesktopRequired`, with its artifact root retained; set `RS_SETUPAPP_REQUIRE_INTERACTIVE_UI=1` on an interactive Windows runner to make that condition fail the gate.

## Signing material

Online update feeds require an RSA private key passed explicitly to the builder:

```powershell
dotnet run --project RS.SetupApp.Builder -- publish-update-feed `
  --package .\artifacts\packages\<product>\<version> `
  --product .\product.json `
  --base-url https://downloads.example.invalid/ `
  --signing-key $env:SETUPAPP_SIGNING_KEY
```

Keep private PEM files outside the repository and release artifacts. CI generates test-only keys in its temporary directory; production keys belong in the release secret store. Only the public key referenced by `update.trustedPublicKeyPath` is bundled.

## Ownership and recovery

An existing installation without `.rs-setup-owner.json` is not removed automatically. After validation, migrate it deliberately with `--claim-legacy`; the installer writes a new ownership marker tied to the installation id. Do not use this flag for an unverified path.

For a current-user product `<id>`, durable state and recovery live under:

- `%LOCALAPPDATA%\RS.SetupApp\InstalledProducts\<id>`
- `%LOCALAPPDATA%\RS.SetupApp\Recovery\<id>`
- `%LOCALAPPDATA%\RS.SetupApp\Logs\<id>`

All-users installs use `%ProgramData%` instead. Interrupted transactions are recovered before a new operation. The UI exposes **Retry recovery** when recovery is blocked; successful uninstall removes the generated install, state, recovery, and configured data roots.

## Silent exit codes and UI smoke

Silent mode returns `0` for success, `2` for cancellation, `3` for an operation or safety failure, and `4` when durable recovery cannot complete. `repair`, `update`, and `uninstall` normally relaunch from a worker copy; automated callers that need the operation's own exit code can pass `--worker` to the generated bundle.

For a visible manual smoke test, generate a bundle and run its `Setup.exe` without `--silent`. Walk Welcome, License, Options, Review, Progress, and Completion; reopen it for Maintenance, Repair, Update, and Uninstall. Use a disposable product id and install root, never a production installation.
