using RS.Widgets;
using RS.Widgets.Controls;
using RS.WPFClient.Enums;
using RS.WPFClient.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.WPFClient.Controls
{
    public partial class RSEmailFilter : UserControl
    {
        public RSEmailFilter()
        {
            InitializeComponent();

        }


        #region 路由事件

        /// <summary>
        /// 邮件筛选路由事件
        /// </summary>
        public static readonly RoutedEvent MailFilteredEvent = EventManager.RegisterRoutedEvent(
            nameof(MailFiltered),
            RoutingStrategy.Bubble,
            typeof(EventHandler<MailFilterEventArgs>),
            typeof(RSEmailFilter));

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

        public static readonly DependencyProperty MailFilteredCommandProperty =
            DependencyProperty.Register(nameof(MailFilteredCommand), typeof(ICommand), typeof(RSEmailFilter), new PropertyMetadata(null));


        public EmailFilterModel EmailFilterModel
        {
            get { return (EmailFilterModel)GetValue(EmailFilterModelProperty); }
            set { SetValue(EmailFilterModelProperty, value); }
        }

        public static readonly DependencyProperty EmailFilterModelProperty =
            DependencyProperty.Register(nameof(EmailFilterModel), typeof(EmailFilterModel), typeof(RSEmailFilter), new PropertyMetadata(null));


        public bool IsAllRead
        {
            get { return (bool)GetValue(IsAllReadProperty); }
            set { SetValue(IsAllReadProperty, value); }
        }

        public static readonly DependencyProperty IsAllReadProperty =
            DependencyProperty.Register(nameof(IsAllRead), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(true));

        public bool IsUnread
        {
            get { return (bool)GetValue(IsUnreadProperty); }
            set { SetValue(IsUnreadProperty, value); }
        }

        public static readonly DependencyProperty IsUnreadProperty =
            DependencyProperty.Register(nameof(IsUnread), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(false));

        public bool IsWithAttachment
        {
            get { return (bool)GetValue(IsWithAttachmentProperty); }
            set { SetValue(IsWithAttachmentProperty, value); }
        }

        public static readonly DependencyProperty IsWithAttachmentProperty =
            DependencyProperty.Register(nameof(IsWithAttachment), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(false));

        public bool IsFromContact
        {
            get { return (bool)GetValue(IsFromContactProperty); }
            set { SetValue(IsFromContactProperty, value); }
        }

        public static readonly DependencyProperty IsFromContactProperty =
            DependencyProperty.Register(nameof(IsFromContact), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(false));


        public bool IsNewestToOldest
        {
            get { return (bool)GetValue(IsNewestToOldestProperty); }
            set { SetValue(IsNewestToOldestProperty, value); }
        }

        public static readonly DependencyProperty IsNewestToOldestProperty =
            DependencyProperty.Register(nameof(IsNewestToOldest), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(true));

        public bool IsOldestToNewest
        {
            get { return (bool)GetValue(IsOldestToNewestProperty); }
            set { SetValue(IsOldestToNewestProperty, value); }
        }

        public static readonly DependencyProperty IsOldestToNewestProperty =
            DependencyProperty.Register(nameof(IsOldestToNewest), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(false));


        public bool IsLargeToSmall
        {
            get { return (bool)GetValue(IsLargeToSmallProperty); }
            set { SetValue(IsLargeToSmallProperty, value); }
        }

        public static readonly DependencyProperty IsLargeToSmallProperty =
            DependencyProperty.Register(nameof(IsLargeToSmall), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(false));

        public bool IsSmallToLarge
        {
            get { return (bool)GetValue(IsSmallToLargeProperty); }
            set { SetValue(IsSmallToLargeProperty, value); }
        }

        public static readonly DependencyProperty IsSmallToLargeProperty =
            DependencyProperty.Register(nameof(IsSmallToLarge), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(false));

        public string MailFilterDes
        {
            get { return (string)GetValue(MailFilterDesProperty); }
            set { SetValue(MailFilterDesProperty, value); }
        }

        public static readonly DependencyProperty MailFilterDesProperty =
            DependencyProperty.Register(nameof(MailFilterDes), typeof(string), typeof(RSEmailFilter), new PropertyMetadata(null));




        public bool HasMailFilterDes
        {
            get { return (bool)GetValue(HasMailFilterDesProperty); }
            set { SetValue(HasMailFilterDesProperty, value); }
        }

        public static readonly DependencyProperty HasMailFilterDesProperty =
            DependencyProperty.Register(nameof(HasMailFilterDes), typeof(bool), typeof(RSEmailFilter), new PropertyMetadata(false));



        public string SizeSortDes
        {
            get { return (string)GetValue(SizeSortDesProperty); }
            set { SetValue(SizeSortDesProperty, value); }
        }

        public static readonly DependencyProperty SizeSortDesProperty =
            DependencyProperty.Register(nameof(SizeSortDes), typeof(string), typeof(RSEmailFilter), new PropertyMetadata(null));


        public string DateSortDes
        {
            get { return (string)GetValue(DateSortDesProperty); }
            set { SetValue(DateSortDesProperty, value); }
        }

        public static readonly DependencyProperty DateSortDesProperty =
            DependencyProperty.Register(nameof(DateSortDes), typeof(string), typeof(RSEmailFilter), new PropertyMetadata(null));

        #endregion


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

            // 更新 EmailFilterModel
            var mailFilterModel = this.GetEmailFilterModel();
            if (this.IsAllRead)
            {
                mailFilterModel.EmailFilterType = EmailFilterType.AllRead;
            }
            else if (this.IsUnread)
            {
                mailFilterModel.EmailFilterType = EmailFilterType.Unread;
            }
            else if (this.IsWithAttachment)
            {
                mailFilterModel.EmailFilterType = EmailFilterType.WithAttachment;
            }
            else if (this.IsFromContact)
            {
                mailFilterModel.EmailFilterType = EmailFilterType.FromContact;
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

            // 更新 EmailFilterModel
            var mailFilterModel = this.GetEmailFilterModel();
            if (this.IsNewestToOldest)
            {
                mailFilterModel.EmailSortType = EmailSortType.NewestToOldest;
            }
            else if (this.IsOldestToNewest)
            {
                mailFilterModel.EmailSortType = EmailSortType.OldestToNewest;
            }
            else if (this.IsLargeToSmall)
            {
                mailFilterModel.EmailSortType = EmailSortType.LargeToSmall;
            }
            else if (this.IsSmallToLarge)
            {
                mailFilterModel.EmailSortType = EmailSortType.SmallToLarge;
            }

            this.HandleSortPropertyChanged();

            this.UpdateSubMenuTags();
        }

        private void PART_BtnClear_Click(object sender, RoutedEventArgs e)
        {
            var mailFilterModel = GetEmailFilterModel();
            mailFilterModel.EmailFilterType = EmailFilterType.AllRead;
            mailFilterModel.EmailSortType = EmailSortType.NewestToOldest;
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
            this.PART_DropdownMenu.IsDropdownOpen = false;
            this.RaiseMailFilterEvent();
        }

        private void HandleSortPropertyChanged()
        {
            this.GenerateMailFilterDes();
            this.PART_DropdownMenu.IsDropdownOpen = false;
            this.RaiseMailFilterEvent();
        }

        private void RaiseMailFilterEvent()
        {
            var mailFilterModel = this.GetEmailFilterModel();
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
            var mailFilterModel = this.GetEmailFilterModel();
            if (mailFilterModel == null)
            {
                return;
            }

            List<string> mailFilterDesList = new List<string>();
            switch (mailFilterModel.EmailFilterType)
            {
                case EmailFilterType.AllRead:
                    break;
                case EmailFilterType.Unread:
                    mailFilterDesList.Add("未读");
                    break;
                case EmailFilterType.WithAttachment:
                    mailFilterDesList.Add("包含附件");
                    break;
                case EmailFilterType.FromContact:
                    mailFilterDesList.Add("来自联系人");
                    break;
            }
            this.DateSortDes = string.Empty;
            this.SizeSortDes = string.Empty;
            switch (mailFilterModel.EmailSortType)
            {
                case EmailSortType.NewestToOldest:
                    break;
                case EmailSortType.OldestToNewest:
                    mailFilterDesList.Add("由旧到新");
                    this.DateSortDes = "由旧到新";
                    break;
                case EmailSortType.LargeToSmall:
                    mailFilterDesList.Add("由大到小");
                    this.SizeSortDes = "由大到小";
                    break;
                case EmailSortType.SmallToLarge:
                    mailFilterDesList.Add("由小到大");
                    this.SizeSortDes = "由小到大";
                    break;
            }

            if (mailFilterDesList.Count > 0)
            {
                this.HasMailFilterDes = true;
                this.MailFilterDes = string.Join(";", mailFilterDesList);
            }
            else
            {
                this.MailFilterDes = null;
                this.HasMailFilterDes = false;
            }
        }

        private EmailFilterModel GetEmailFilterModel()
        {
            if (this.EmailFilterModel == null)
            {
                this.EmailFilterModel = new EmailFilterModel();
                this.EmailFilterModel.EmailFilterType = EmailFilterType.AllRead;
                this.EmailFilterModel.EmailSortType = EmailSortType.NewestToOldest;
            }
            return this.EmailFilterModel;
        }
    }
}
