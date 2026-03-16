namespace RS.SetupApp.ViewModels;

public static class SetupStatusTranslator
{
    public static string Translate(string message, UiLanguage language, SetupLanguageResources resources)
    {
        if (language == UiLanguage.English || string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return message switch
        {
            "Ready." => resources.ReadyStatusText,
            "Not installed" => resources.NotInstalledStatusText,
            "Not checked" => resources.NotCheckedStatusText,
            "No updates" => resources.NoUpdatesStatusText,
            "Check failed" => resources.CheckFailedStatusText,
            "No update is available." => resources.NoUpdateAvailableStatusText,
            "Running install..." => resources.FormatRunningOperation(RS.SetupApp.Core.SetupMode.Install),
            "Running repair..." => resources.FormatRunningOperation(RS.SetupApp.Core.SetupMode.Repair),
            "Running update..." => resources.FormatRunningOperation(RS.SetupApp.Core.SetupMode.Update),
            "Running uninstall..." => resources.FormatRunningOperation(RS.SetupApp.Core.SetupMode.Uninstall),
            "Installation completed successfully." => "安装已成功完成。",
            "Repair completed successfully." => "修复已成功完成。",
            "Update completed successfully." => "更新已成功完成。",
            "Uninstall completed successfully." => "卸载已成功完成。",
            "Product is not installed." => "当前产品尚未安装。",
            "Load product manifest" => "正在加载产品清单",
            "Validate product schema" => "正在校验产品结构",
            "Validate product manifest" => "正在校验产品语义",
            "Load installed state" => "正在读取已安装状态",
            "Prepare working directory" => "正在准备工作目录",
            "Download update manifest" => "正在解析更新清单",
            "Resolve package" => "正在解析安装包",
            "Resolve operation state" => "正在解析操作状态",
            "Validate package archive" => "正在校验安装包哈希",
            "Extract package" => "正在解压安装包",
            "Validate extracted files" => "正在校验解压后的文件",
            "Validate install target" => "正在校验安装目标",
            "Run install extensions" => "正在执行安装前扩展",
            "Close running application" => "正在关闭运行中的程序",
            "Backup current installation" => "正在备份当前安装",
            "Deploy application files" => "正在部署应用文件",
            "Deploy maintenance runtime" => "正在部署维护运行时",
            "Apply system integrations" => "正在写入系统集成项",
            "Write installed state" => "正在写入已安装状态",
            "Finalize install extensions" => "正在执行安装后扩展",
            "Cleanup working directory" => "正在清理临时目录",
            "Run uninstall extensions" => "正在执行卸载前扩展",
            "Remove system integrations" => "正在移除系统集成项",
            "Remove installed files" => "正在删除已安装文件",
            "Remove product data" => "正在清理产品数据",
            "Remove installed state" => "正在删除已安装状态",
            "Finalize uninstall extensions" => "正在执行卸载后扩展",
            _ when message.StartsWith("Update ", StringComparison.Ordinal) && message.EndsWith(" is available.", StringComparison.Ordinal) =>
                string.Format(resources.UpdateAvailableStatusTemplate, message["Update ".Length..^" is available.".Length]),
            _ => message
        };
    }
}
