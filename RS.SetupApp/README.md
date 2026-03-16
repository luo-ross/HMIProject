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
