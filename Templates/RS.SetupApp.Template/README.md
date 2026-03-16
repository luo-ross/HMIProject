# RS.SetupApp 模板

这个模板用于把你的 Windows 桌面程序快速接入 `RS.SetupApp`，生成可交付的通用安装器。

## 快速开始

1. 修改 `product.json`，填入你的产品信息、安装配置和更新地址。
2. 替换 `assets/icon.ico` 和 `LICENSE.txt`，换成你自己的品牌资源和许可协议。
3. 将程序发布产物放到本地 `publish/` 目录，或者在构建时直接指向你的项目文件。
4. 运行 `build-installer.ps1`。
5. 使用生成的 `artifacts/installer/...` 目录进行分发。

## 模板内包含的文件

- `product.json`：产品清单，供 Builder 和 Runtime 读取。
- `product.schema.json`：产品清单结构约束，构建期和运行期都会校验。
- `build-installer.ps1`：示例打包脚本。
- `publish/README.md`：说明 `publish/` 目录中应该放什么。
- `assets/icon.ico`：可直接替换的占位图标。
- `assets/README.md`：说明品牌资源目录的用途。

## 推荐接入方式

1. 先执行一次应用发布，确认主程序可以独立运行。
2. 再配置 `product.json` 中的主程序、安装路径、快捷方式、文件关联和更新源。
3. 最后运行 Builder，生成离线安装器、更新包和更新清单。

## 生成产物

执行脚本后，默认会得到这些文件：

- `artifacts/packages/<product>-<version>.zip`
- `artifacts/packages/package.manifest.json`
- `artifacts/packages/checksums.txt`
- `artifacts/packages/latest.json`
- `artifacts/installer/Setup.exe`

## 注意事项

- `publish/` 目录中应放最终发布文件，而不是源码。
- `LICENSE.txt` 建议替换为你自己的正式协议文本。
- 如果你使用在线更新，请把 `latest.json` 和安装包部署到可访问的 HTTPS 地址。
