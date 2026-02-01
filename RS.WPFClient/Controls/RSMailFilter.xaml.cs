using RS.Commons.Extend;
using RS.Widgets;
using RS.Widgets.Controls;
using RS.WPFClient.Enums;
using RS.WPFClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS.WPFClient.Controls
{
    /// <summary>
    /// RSMailFilter.xaml 的交互逻辑
    /// 使用 ContextMenu 替代 Popup，左键点击显示菜单
    /// </summary>
    public partial class RSMailFilter : ToggleButton
    {
        public RSMailFilter()
        {
            InitializeComponent();
            this.Loaded += RSMailFilter_Loaded;
        }


        #region 路由事件

        /// <summary>
        /// 邮件筛选路由事件
        /// </summary>
        public static readonly RoutedEvent MailFilteredEvent = EventManager.RegisterRoutedEvent(
            nameof(MailFiltered),
            RoutingStrategy.Bubble,
            typeof(EventHandler<MailFilterEventArgs>),
            typeof(RSMailFilter));

        /// <summary>
        /// 邮件筛选事件
        /// </summary>
        public event EventHandler<MailFilterEventArgs> MailFiltered
        {
            add { AddHandler(MailFilteredEvent, value); }
            remove { RemoveHandler(MailFilteredEvent, value); }
        }

        #endregion

        #region 依赖属性

        /// <summary>
        /// 邮件筛选命令（用于MVVM绑定）
        /// </summary>
        public ICommand MailFilteredCommand
        {
            get { return (ICommand)GetValue(MailFilteredCommandProperty); }
            set { SetValue(MailFilteredCommandProperty, value); }
        }

        /// <summary>
        /// 邮件筛选命令依赖属性
        /// </summary>
        public static readonly DependencyProperty MailFilteredCommandProperty =
            DependencyProperty.Register(nameof(MailFilteredCommand), typeof(ICommand), typeof(RSMailFilter), new PropertyMetadata(null));


        public MailFilterModel MailFilterModel
        {
            get { return (MailFilterModel)GetValue(MailFilterModelProperty); }
            set { SetValue(MailFilterModelProperty, value); }
        }

        public static readonly DependencyProperty MailFilterModelProperty =
            DependencyProperty.Register(nameof(MailFilterModel), typeof(MailFilterModel), typeof(RSMailFilter), new PropertyMetadata(null));



        public bool IsAllRead
        {
            get { return (bool)GetValue(IsAllReadProperty); }
            set { SetValue(IsAllReadProperty, value); }
        }
        public static readonly DependencyProperty IsAllReadProperty =
            DependencyProperty.Register(nameof(IsAllRead), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(true));

        public bool IsUnread
        {
            get { return (bool)GetValue(IsUnreadProperty); }
            set { SetValue(IsUnreadProperty, value); }
        }

        public static readonly DependencyProperty IsUnreadProperty =
            DependencyProperty.Register(nameof(IsUnread), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false));

        public bool IsWithAttachment
        {
            get { return (bool)GetValue(IsWithAttachmentProperty); }
            set { SetValue(IsWithAttachmentProperty, value); }
        }

        public static readonly DependencyProperty IsWithAttachmentProperty =
            DependencyProperty.Register(nameof(IsWithAttachment), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false));

        public bool IsFromContact
        {
            get { return (bool)GetValue(IsFromContactProperty); }
            set { SetValue(IsFromContactProperty, value); }
        }

        public static readonly DependencyProperty IsFromContactProperty =
            DependencyProperty.Register(nameof(IsFromContact), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false));



        public bool IsNewestToOldest
        {
            get { return (bool)GetValue(IsNewestToOldestProperty); }
            set { SetValue(IsNewestToOldestProperty, value); }
        }

        public static readonly DependencyProperty IsNewestToOldestProperty =
            DependencyProperty.Register(nameof(IsNewestToOldest), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(true));

        public bool IsOldestToNewest
        {
            get { return (bool)GetValue(IsOldestToNewestProperty); }
            set { SetValue(IsOldestToNewestProperty, value); }
        }

        public static readonly DependencyProperty IsOldestToNewestProperty =
            DependencyProperty.Register(nameof(IsOldestToNewest), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false));


        public bool IsLargeToSmall
        {
            get { return (bool)GetValue(IsLargeToSmallProperty); }
            set { SetValue(IsLargeToSmallProperty, value); }
        }

        public static readonly DependencyProperty IsLargeToSmallProperty =
            DependencyProperty.Register(nameof(IsLargeToSmall), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false));

        public bool IsSmallToLarge
        {
            get { return (bool)GetValue(IsSmallToLargeProperty); }
            set { SetValue(IsSmallToLargeProperty, value); }
        }

        public static readonly DependencyProperty IsSmallToLargeProperty =
            DependencyProperty.Register(nameof(IsSmallToLarge), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false));

        public string MailFilterDes
        {
            get { return (string)GetValue(MailFilterDesProperty); }
            set { SetValue(MailFilterDesProperty, value); }
        }

        public static readonly DependencyProperty MailFilterDesProperty =
            DependencyProperty.Register(nameof(MailFilterDes), typeof(string), typeof(RSMailFilter), new PropertyMetadata(null));


        public string SizeSortDes
        {
            get { return (string)GetValue(SizeSortDesProperty); }
            set { SetValue(SizeSortDesProperty, value); }
        }

        public static readonly DependencyProperty SizeSortDesProperty =
            DependencyProperty.Register(nameof(SizeSortDes), typeof(string), typeof(RSMailFilter), new PropertyMetadata(null));




        public string DateSortDes
        {
            get { return (string)GetValue(DateSortDesProperty); }
            set { SetValue(DateSortDesProperty, value); }
        }

        public static readonly DependencyProperty DateSortDesProperty =
            DependencyProperty.Register(nameof(DateSortDes), typeof(string), typeof(RSMailFilter), new PropertyMetadata(null));
        #endregion


        private void RSMailFilter_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化 MenuItem 的选中状态
            this.UpdateMenuItemCheckState();

            // 订阅 ContextMenu 关闭事件，同步 IsChecked 状态
            if (this.ContextMenu != null)
            {
                this.ContextMenu.Closed += this.ContextMenu_Closed;
            }
        }

        /// <summary>
        /// ContextMenu 关闭事件处理
        /// </summary>
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            // 菜单关闭时，重置 ToggleButton 的 IsChecked 状态
            this.SetCurrentValue(IsCheckedProperty, false);
        }

        /// <summary>
        /// 重写 OnClick，左键点击时切换 ContextMenu 显示/隐藏
        /// </summary>
        protected override void OnClick()
        {
            // 先调用 base.OnClick() 来切换 IsChecked 状态
            base.OnClick();

            // 根据切换后的 IsChecked 状态决定打开或关闭菜单
            if (this.IsChecked == true)
            {
                // ToggleButton 现在是 checked 状态，打开菜单
                this.ShowContextMenu();
            }
            else
            {
                // ToggleButton 现在是 unchecked 状态，关闭菜单
                this.CloseContextMenu();
            }
        }

        /// <summary>
        /// 显示 ContextMenu
        /// </summary>
        private void ShowContextMenu()
        {
            if (this.ContextMenu == null)
            {
                return;
            }

            // 更新菜单项选中状态
            this.UpdateMenuItemCheckState();

            this.ContextMenu.PlacementTarget = this;
            this.ContextMenu.Placement = PlacementMode.Bottom;
            this.ContextMenu.IsOpen = true;
        }

        /// <summary>
        /// 更新 MenuItem 的选中状态
        /// </summary>
        private void UpdateMenuItemCheckState()
        {
            if (this.BtnAllRead != null)
            {
                this.BtnAllRead.IsChecked = this.IsAllRead;
            }
            if (this.BtnUnread != null)
            {
                this.BtnUnread.IsChecked = this.IsUnread;
            }
            if (this.BtnWithAttachment != null)
            {
                this.BtnWithAttachment.IsChecked = this.IsWithAttachment;
            }
            if (this.BtnFromContact != null)
            {
                this.BtnFromContact.IsChecked = this.IsFromContact;
            }
            if (this.FromNewToOld != null)
            {
                this.FromNewToOld.IsChecked = this.IsNewestToOldest;
            }
            if (this.FromOldToNew != null)
            {
                this.FromOldToNew.IsChecked = this.IsOldestToNewest;
            }
            if (this.BtnFromLargeToSmall != null)
            {
                this.BtnFromLargeToSmall.IsChecked = this.IsLargeToSmall;
            }
            if (this.BtnFromSmallToLarge != null)
            {
                this.BtnFromSmallToLarge.IsChecked = this.IsSmallToLarge;
            }

            // 更新子菜单的 Tag（显示当前选中项的描述）
            this.UpdateSubMenuTags();
        }

        /// <summary>
        /// 更新子菜单的 Tag 显示
        /// </summary>
        private void UpdateSubMenuTags()
        {
            if (this.PART_MailDateMenu != null)
            {
                if (this.IsOldestToNewest)
                {
                    this.PART_MailDateMenu.Tag = "由旧到新";
                }
                else
                {
                    this.PART_MailDateMenu.Tag = string.Empty;
                }
            }

            if (this.PART_MailSizeMenu != null)
            {
                if (this.IsLargeToSmall)
                {
                    this.PART_MailSizeMenu.Tag = "由大到小";
                }
                else if (this.IsSmallToLarge)
                {
                    this.PART_MailSizeMenu.Tag = "由小到大";
                }
                else
                {
                    this.PART_MailSizeMenu.Tag = string.Empty;
                }
            }
        }

        /// <summary>
        /// 筛选菜单项点击事件
        /// </summary>
        private void FilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            // 实现单选逻辑：取消其他筛选项的选中状态
            this.IsAllRead = menuItem == this.BtnAllRead;
            this.IsUnread = menuItem == this.BtnUnread;
            this.IsWithAttachment = menuItem == this.BtnWithAttachment;
            this.IsFromContact = menuItem == this.BtnFromContact;

            // 更新 MailFilterModel
            var mailFilterModel = this.GetMailFilterModel();
            if (this.IsAllRead)
            {
                mailFilterModel.MailFilterType = MailFilterType.AllRead;
            }
            else if (this.IsUnread)
            {
                mailFilterModel.MailFilterType = MailFilterType.Unread;
            }
            else if (this.IsWithAttachment)
            {
                mailFilterModel.MailFilterType = MailFilterType.WithAttachment;
            }
            else if (this.IsFromContact)
            {
                mailFilterModel.MailFilterType = MailFilterType.FromContact;
            }

            this.HandleFilterPropertyChanged();
        }

        /// <summary>
        /// 排序菜单项点击事件
        /// </summary>
        private void SortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            // 实现单选逻辑：取消其他排序项的选中状态
            this.IsNewestToOldest = menuItem == this.FromNewToOld;
            this.IsOldestToNewest = menuItem == this.FromOldToNew;
            this.IsLargeToSmall = menuItem == this.BtnFromLargeToSmall;
            this.IsSmallToLarge = menuItem == this.BtnFromSmallToLarge;

            // 更新 MailFilterModel
            var mailFilterModel = this.GetMailFilterModel();
            if (this.IsNewestToOldest)
            {
                mailFilterModel.MailSortType = MailSortType.NewestToOldest;
            }
            else if (this.IsOldestToNewest)
            {
                mailFilterModel.MailSortType = MailSortType.OldestToNewest;
            }
            else if (this.IsLargeToSmall)
            {
                mailFilterModel.MailSortType = MailSortType.LargeToSmall;
            }
            else if (this.IsSmallToLarge)
            {
                mailFilterModel.MailSortType = MailSortType.SmallToLarge;
            }

            this.HandleSortPropertyChanged();
        }

        private void PART_BtnClear_Click(object sender, RoutedEventArgs e)
        {
            var mailFilterModel = GetMailFilterModel();
            mailFilterModel.MailFilterType = MailFilterType.AllRead;
            mailFilterModel.MailSortType = MailSortType.NewestToOldest;
            this.IsAllRead = true;
            this.IsUnread = false;
            this.IsWithAttachment = false;
            this.IsFromContact = false;
            this.IsNewestToOldest = true;
            this.IsOldestToNewest = false;
            this.IsLargeToSmall = false;
            this.IsSmallToLarge = false;
            this.GenerateMailFilterDes();
            this.RaiseMailFilterEvent();
        }

        private void HandleFilterPropertyChanged()
        {
            this.GenerateMailFilterDes();
            this.CloseContextMenu();
            this.RaiseMailFilterEvent();
        }

        private void HandleSortPropertyChanged()
        {
            this.GenerateMailFilterDes();
            this.CloseContextMenu();
            this.RaiseMailFilterEvent();
        }

        /// <summary>
        /// 关闭 ContextMenu
        /// </summary>
        private void CloseContextMenu()
        {
            if (this.ContextMenu != null)
            {
                this.ContextMenu.IsOpen = false;
            }
            // 重置 ToggleButton 的 IsChecked 状态
            this.SetCurrentValue(IsCheckedProperty, false);
        }

        private void RaiseMailFilterEvent()
        {
            var mailFilterModel = this.GetMailFilterModel();
            //触发路由事件
            MailFilterEventArgs eventArgs = new MailFilterEventArgs(
                MailFilteredEvent,
                mailFilterModel);
            RaiseEvent(eventArgs);
            //执行Command（如果设置了）
            if (MailFilteredCommand != null && MailFilteredCommand.CanExecute(mailFilterModel))
            {
                MailFilteredCommand.Execute(mailFilterModel);
            }
        }

        private void GenerateMailFilterDes()
        {
            var mailFilterModel = this.GetMailFilterModel();
            if (mailFilterModel == null)
            {
                return;
            }

            List<string> mailFilterDesList = new List<string>();
            switch (mailFilterModel.MailFilterType)
            {
                case MailFilterType.AllRead:
                    break;
                case MailFilterType.Unread:
                    mailFilterDesList.Add("未读");
                    break;
                case MailFilterType.WithAttachment:
                    mailFilterDesList.Add("包含附件");
                    break;
                case MailFilterType.FromContact:
                    mailFilterDesList.Add("来自联系人");
                    break;
            }
            this.DateSortDes = string.Empty;
            this.SizeSortDes = string.Empty;
            switch (mailFilterModel.MailSortType)
            {
                case MailSortType.NewestToOldest:
                    break;
                case MailSortType.OldestToNewest:
                    mailFilterDesList.Add("由旧到新");
                    this.DateSortDes = "由旧到新";
                    break;
                case MailSortType.LargeToSmall:
                    mailFilterDesList.Add("由大到小");
                    this.SizeSortDes = "由大到小";
                    break;
                case MailSortType.SmallToLarge:
                    mailFilterDesList.Add("由小到大");
                    this.SizeSortDes = "由小到大";
                    break;
            }

            if (mailFilterDesList.Count > 0)
            {
                this.SetCurrentValue(ToggleButton.ContentProperty, string.Join(';', mailFilterDesList));
            }
            else
            {
                this.SetCurrentValue(ToggleButton.ContentProperty, null);
            }
        }

        private MailFilterModel GetMailFilterModel()
        {
            if (this.MailFilterModel == null)
            {
                this.MailFilterModel = new MailFilterModel();
                this.MailFilterModel.MailFilterType = MailFilterType.AllRead;
                this.MailFilterModel.MailSortType = MailSortType.NewestToOldest;
            }
            return this.MailFilterModel;
        }



    }
}
