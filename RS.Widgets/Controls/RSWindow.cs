using RS.Commons;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace RS.Widgets.Controls
{
    public class RSWindow : RSWindowBase
    {
        private Button PART_Minimize;
        private Button PART_BtnMaxRestore;
        private Button PART_BtnClose;
        private RSUserControl PART_WinContentHost;

        public IMessageBox MessageBox
        {
            get
            {
                return this.PART_WinContentHost.MessageBox;
            }
        }

        public IMessageBox WinMessageBox
        {
            get
            {
                return new RSWinMessageBox()
                {
                    Owner = this,
                    Width = 350,
                    Height = 250
                };
            }
        }


        static RSWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSWindow), new FrameworkPropertyMetadata(typeof(RSWindow)));
        }

        public RSWindow()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow, CanCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeRestoreWindow, CanMaximizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, MaximizeRestoreWindow, CanRestoreWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu, CanShowSystemMenu));
            // 添加命令绑定
            this.CommandBindings.Add(new CommandBinding(RSCommands.CleanTextCommand, CleanTextText));
        }


        [Description("服务连接是否成功")]
        public bool? IsServerConnectSuccess
        {
            get { return (bool?)GetValue(IsServerConnectSuccessProperty); }
            set { SetValue(IsServerConnectSuccessProperty, value); }
        }

        public static readonly DependencyProperty IsServerConnectSuccessProperty =
            DependencyProperty.Register("IsServerConnectSuccess", typeof(bool?), typeof(RSWindow), new PropertyMetadata(null));


        public async Task<OperateResult> InvokeLoadingActionAsync(Func<Task<OperateResult>> func, LoadingConfig loadingConfig = null)
        {
            return await this.PART_WinContentHost.InvokeLoadingActionAsync(func, loadingConfig);
        }

        private void CleanTextText(object sender, ExecutedRoutedEventArgs e)
        {
            RSCommands.CleanText(e.Parameter);
        }

        private void CanWindowMove(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        private void WindowMaxRestore(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.PART_BtnMaxRestore.Command != null && this.PART_BtnMaxRestore.Command.CanExecute(null))
            {
                if (!(this.ResizeMode == ResizeMode.NoResize))
                {
                    this.PART_BtnMaxRestore.Command.Execute(null);
                }
            }
        }

        private void CanShowSystemMenu(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.ShowSystemMenu(this, this.PointToScreen(Mouse.GetPosition(this)));
        }





        private void CanRestoreWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WindowState == WindowState.Maximized;
        }

        private void CanMaximizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WindowState == WindowState.Normal;
        }

        private void MaximizeRestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CanCloseWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }


        /// <summary>
        /// 显示模态
        /// </summary>
        public void ShowModal(object modalContent)
        {
            this.PART_WinContentHost.ShowModal(modalContent);
        }

        /// <summary>
        /// 关闭模态
        /// </summary>
        public void HideModal()
        {
            this.PART_WinContentHost.HideModal();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Minimize = this.GetTemplateChild(nameof(this.PART_Minimize)) as Button;
            this.PART_BtnMaxRestore = this.GetTemplateChild(nameof(this.PART_BtnMaxRestore)) as Button;
            this.PART_BtnClose = this.GetTemplateChild(nameof(this.PART_BtnClose)) as Button;
            this.PART_WinContentHost = this.GetTemplateChild(nameof(this.PART_WinContentHost)) as RSUserControl;
        }




        [Description("自定义标题栏内容")]
        [Browsable(false)]
        public object CaptionContent
        {
            get { return (object)GetValue(CaptionContentProperty); }
            set { SetValue(CaptionContentProperty, value); }
        }

        public static readonly DependencyProperty CaptionContentProperty =
            DependencyProperty.Register("CaptionContent", typeof(object), typeof(RSWindow), new PropertyMetadata(null));



        [Description("标题栏高度设置")]
        [Browsable(true)]
        [Category("自定义窗口样式")]
        public double CaptionHeight
        {
            get { return (double)GetValue(CaptionHeightProperty); }
            set { SetValue(CaptionHeightProperty, value); }
        }

        public static readonly DependencyProperty CaptionHeightProperty =
            DependencyProperty.Register("CaptionHeight", typeof(double), typeof(RSWindow), new PropertyMetadata(30D));



        [Description("是否沉浸式")]
        [Browsable(true)]
        [Category("自定义窗口样式")]
        public bool IsFitSystem
        {
            get { return (bool)GetValue(IsFitSystemProperty); }
            set { SetValue(IsFitSystemProperty, value); }
        }
        public static readonly DependencyProperty IsFitSystemProperty =
            DependencyProperty.Register("IsFitSystem", typeof(bool), typeof(RSWindow), new PropertyMetadata(false));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsMaxsizedFullScreenProperty)
            {
                if (this.IsMaxsizedFullScreen)
                {
                    SystemCommands.MaximizeWindow(this);
                }
                else
                {
                    SystemCommands.RestoreWindow(this);
                }

            }

        }
    }
}
