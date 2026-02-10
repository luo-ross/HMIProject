using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using RS.Widgets;
using RS.Widgets.Controls;
using RS.WPFClient.Enums;
using RS.WPFClient.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.WPFClient.Views
{

    public partial class EmailDetailView : UserControl
    {
        public EmailDetailView()
        {
            InitializeComponent();

            InitWebView2WithHideTip();
        }

       

        private async void InitWebView2WithHideTip()
        {
            try
            {
                // 1. 配置Chromium内核启动参数，核心：禁用状态栏
                var envOptions = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = "--disable-status-bar"
                    // 多个参数用空格分隔，后续可加其他参数
                };

                // 2. 创建自定义环境
                var webView2Env = await CoreWebView2Environment.CreateAsync(null, null, envOptions);

                // 3. 初始化WebView2
                await webView.EnsureCoreWebView2Async(webView2Env);
                // 第一层：控件级禁用状态栏（CoreWebView2.Settings）
                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView.CoreWebView2.Settings.AreHostObjectsAllowed = false;
                webView.CoreWebView2.Settings.IsScriptEnabled = false;
                webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                webView.CoreWebView2.Settings.IsWebMessageEnabled = false;
                webView.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                webView.CoreWebView2.Settings.IsPinchZoomEnabled = false;
                webView.CoreWebView2.Settings.IsNonClientRegionSupportEnabled = false;
                webView.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
                webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
                webView.CoreWebView2.Settings.IsReputationCheckingRequired = false;
                webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化失败：" + ex.Message);
            }
        }
    }
}
