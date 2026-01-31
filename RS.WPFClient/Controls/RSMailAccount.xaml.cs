using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.WPFClient.Controls
{
    /// <summary>
    /// RSMailAccount.xaml 的交互逻辑
    /// </summary>
    public partial class RSMailAccount : UserControl
    {
        public RSMailAccount()
        {
            InitializeComponent();
        }

        #region 依赖属性

        public string Account
        {
            get
            {
                return (string)GetValue(AccountProperty);
            }
            set
            {
                SetValue(AccountProperty, value);
            }
        }

        public static readonly DependencyProperty AccountProperty =
            DependencyProperty.Register(nameof(Account), typeof(string), typeof(RSMailAccount), new PropertyMetadata(string.Empty));

        public string Email
        {
            get
            {
                return (string)GetValue(EmailProperty);
            }
            set
            {
                SetValue(EmailProperty, value);
            }
        }

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register(nameof(Email), typeof(string), typeof(RSMailAccount), new PropertyMetadata(string.Empty));

        public ImageSource Avatar
        {
            get
            {
                return (ImageSource)GetValue(AvatarProperty);
            }
            set
            {
                SetValue(AvatarProperty, value);
            }
        }

        public static readonly DependencyProperty AvatarProperty =
            DependencyProperty.Register(nameof(Avatar), typeof(ImageSource), typeof(RSMailAccount), new PropertyMetadata(null));

        public IEnumerable RecentInteractions
        {
            get
            {
                return (IEnumerable)GetValue(RecentInteractionsProperty);
            }
            set
            {
                SetValue(RecentInteractionsProperty, value);
            }
        }

        public static readonly DependencyProperty RecentInteractionsProperty =
            DependencyProperty.Register(nameof(RecentInteractions), typeof(IEnumerable), typeof(RSMailAccount), new PropertyMetadata(null));

        public ICommand ViewAllCommand
        {
            get
            {
                return (ICommand)GetValue(ViewAllCommandProperty);
            }
            set
            {
                SetValue(ViewAllCommandProperty, value);
            }
        }

        public static readonly DependencyProperty ViewAllCommandProperty =
            DependencyProperty.Register(nameof(ViewAllCommand), typeof(ICommand), typeof(RSMailAccount), new PropertyMetadata(null));

        public ICommand SendEmailCommand
        {
            get
            {
                return (ICommand)GetValue(SendEmailCommandProperty);
            }
            set
            {
                SetValue(SendEmailCommandProperty, value);
            }
        }

        public static readonly DependencyProperty SendEmailCommandProperty =
            DependencyProperty.Register(nameof(SendEmailCommand), typeof(ICommand), typeof(RSMailAccount), new PropertyMetadata(null));

        #endregion

        #region 路由事件

        public static readonly RoutedEvent ViewAllClickEvent = EventManager.RegisterRoutedEvent(
            nameof(ViewAllClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSMailAccount));

        public event RoutedEventHandler ViewAllClick
        {
            add
            {
                AddHandler(ViewAllClickEvent, value);
            }
            remove
            {
                RemoveHandler(ViewAllClickEvent, value);
            }
        }

        public static readonly RoutedEvent SendEmailClickEvent = EventManager.RegisterRoutedEvent(
            nameof(SendEmailClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSMailAccount));

        public event RoutedEventHandler SendEmailClick
        {
            add
            {
                AddHandler(SendEmailClickEvent, value);
            }
            remove
            {
                RemoveHandler(SendEmailClickEvent, value);
            }
        }

        #endregion

        private void PART_BtnViewAll_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ViewAllClickEvent));

            if (ViewAllCommand != null && ViewAllCommand.CanExecute(null))
            {
                ViewAllCommand.Execute(null);
            }
        }

        private void PART_BtnCopyEmail_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Email))
            {
                Clipboard.SetText(Email);
            }
        }

        private void PART_EmailText_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SendEmailClickEvent));

            if (SendEmailCommand != null && SendEmailCommand.CanExecute(Email))
            {
                SendEmailCommand.Execute(Email);
            }
        }
    }
}
