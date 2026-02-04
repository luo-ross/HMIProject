using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// Helper for WebView2 HTML content binding with modern Chromium engine
    /// </summary>
    public static class WebView2Helper
    {
        #region HtmlContent Property
        public static readonly DependencyProperty HtmlContentProperty =
            DependencyProperty.RegisterAttached(
                "HtmlContent",
                typeof(string),
                typeof(WebView2Helper),
                new PropertyMetadata(null, OnHtmlContentChanged));

        public static string GetHtmlContent(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlContentProperty);
        }

        public static void SetHtmlContent(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlContentProperty, value);
        }

        private static async void OnHtmlContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebView2 webView)
            {
                string? html = e.NewValue as string;

                // 确保 WebView2 已初始化
                if (webView.CoreWebView2 == null)
                {
                    webView.Loaded += async (s, args) =>
                    {
                        try
                        {
                            await webView.EnsureCoreWebView2Async();
                            NavigateToHtml(webView, html);
                        }
                        catch
                        {
                            // WebView2 Runtime not installed
                        }
                    };
                }
                else
                {
                    NavigateToHtml(webView, html);
                }
            }
        }

        private static void NavigateToHtml(WebView2 webView, string? html)
        {
            if (string.IsNullOrEmpty(html))
            {
                webView.CoreWebView2?.NavigateToString("<html><body></body></html>");
                return;
            }

            // Wrap with UTF-8 document head and custom styles
            string fullHtml = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <style>
        /* Body styling */
        body {{
            font-family: 'Segoe UI', 'Microsoft YaHei', sans-serif;
            font-size: 14px;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 16px;
            background: #fff;
        }}
        
        /* Custom scrollbar */
        ::-webkit-scrollbar {{
            width: 8px;
            height: 8px;
        }}
        ::-webkit-scrollbar-track {{
            background: #f1f1f1;
            border-radius: 4px;
        }}
        ::-webkit-scrollbar-thumb {{
            background: #c1c1c1;
            border-radius: 4px;
        }}
        ::-webkit-scrollbar-thumb:hover {{
            background: #a8a8a8;
        }}
        
        /* Link styling */
        a {{
            color: #0078d4;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
        
        /* Image max width */
        img {{
            max-width: 100%;
            height: auto;
        }}
    </style>
</head>
<body>
{html}
</body>
</html>";

            webView.CoreWebView2?.NavigateToString(fullHtml);
        }
        #endregion

        #region OpenExternalLinks Property
        public static readonly DependencyProperty OpenExternalLinksProperty =
            DependencyProperty.RegisterAttached(
                "OpenExternalLinks",
                typeof(bool),
                typeof(WebView2Helper),
                new PropertyMetadata(false, OnOpenExternalLinksChanged));

        public static bool GetOpenExternalLinks(DependencyObject obj)
        {
            return (bool)obj.GetValue(OpenExternalLinksProperty);
        }

        public static void SetOpenExternalLinks(DependencyObject obj, bool value)
        {
            obj.SetValue(OpenExternalLinksProperty, value);
        }

        private static void OnOpenExternalLinksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebView2 webView)
            {
                if ((bool)e.NewValue)
                {
                    webView.Loaded += WebView_Loaded;
                }
                else
                {
                    webView.Loaded -= WebView_Loaded;
                }
            }
        }

        private static async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebView2 webView)
            {
                try
                {
                    await webView.EnsureCoreWebView2Async();
                    webView.CoreWebView2.NewWindowRequested += (s, args) =>
                    {
                        // Open in default browser instead of new window
                        args.Handled = true;
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = args.Uri,
                                UseShellExecute = true
                            });
                        }
                        catch { }
                    };

                    webView.CoreWebView2.NavigationStarting += (s, args) =>
                    {
                        // Allow data: and about: schemes for initial content
                        if (args.Uri.StartsWith("data:") || args.Uri.StartsWith("about:"))
                        {
                            return;
                        }

                        // Block navigation and open in default browser
                        if (args.Uri.StartsWith("http://") || args.Uri.StartsWith("https://"))
                        {
                            args.Cancel = true;
                            try
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = args.Uri,
                                    UseShellExecute = true
                                });
                            }
                            catch { }
                        }
                    };
                }
                catch { }
            }
        }
        #endregion
    }
}
