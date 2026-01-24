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
using ZXing.OneD;

namespace RS.WPFClient.Controls
{
    /// <summary>
    /// RSMailFilter.xaml 的交互逻辑
    /// </summary>
    public partial class RSMailFilter : ToggleButton
    {
        private Popup PART_Popup;
        private RSDropdown PART_MailSize;
        private RSDropdown PART_MailDate;
        private Window ParentWindow;
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
            DependencyProperty.Register(nameof(IsAllRead), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(true, IsAllReadPropertyChanged));

        private static void IsAllReadPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mailFilter = d as RSMailFilter;
            if (!e.NewValue.ToBool() || mailFilter == null)
            {
                return;
            }
            mailFilter.GetMailFilterModel().MailFilterType = MailFilterType.AllRead;
            mailFilter.HandleFilterPropertyChanged();
        }

        public bool IsUnread
        {
            get { return (bool)GetValue(IsUnreadProperty); }
            set { SetValue(IsUnreadProperty, value); }
        }

        public static readonly DependencyProperty IsUnreadProperty =
            DependencyProperty.Register(nameof(IsUnread), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false, OnIsUnreadPropertyChanged));

        private static void OnIsUnreadPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mailFilter = d as RSMailFilter;
            if (!e.NewValue.ToBool() || mailFilter == null)
            {
                return;
            }
            mailFilter.GetMailFilterModel().MailFilterType = MailFilterType.Unread;
            mailFilter.HandleFilterPropertyChanged();
        }

        public bool IsWithAttachment
        {
            get { return (bool)GetValue(IsWithAttachmentProperty); }
            set { SetValue(IsWithAttachmentProperty, value); }
        }

        public static readonly DependencyProperty IsWithAttachmentProperty =
            DependencyProperty.Register(nameof(IsWithAttachment), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false, OnIsWithAttachmentPropertyChanged));

        private static void OnIsWithAttachmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mailFilter = d as RSMailFilter;
            if (!e.NewValue.ToBool() || mailFilter == null)
            {
                return;
            }

            mailFilter.GetMailFilterModel().MailFilterType = MailFilterType.WithAttachment;
            mailFilter?.HandleFilterPropertyChanged();
        }

        public bool IsFromContact
        {
            get { return (bool)GetValue(IsFromContactProperty); }
            set { SetValue(IsFromContactProperty, value); }
        }

        public static readonly DependencyProperty IsFromContactProperty =
            DependencyProperty.Register(nameof(IsFromContact), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false, OnIsFromContactPropertyChanged));

        private static void OnIsFromContactPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mailFilter = d as RSMailFilter;
            if (!e.NewValue.ToBool() || mailFilter == null)
            {
                return;
            }
            mailFilter.GetMailFilterModel().MailFilterType = MailFilterType.FromContact;
            mailFilter.HandleFilterPropertyChanged();
        }



        public bool IsNewestToOldest
        {
            get { return (bool)GetValue(IsNewestToOldestProperty); }
            set { SetValue(IsNewestToOldestProperty, value); }
        }

        public static readonly DependencyProperty IsNewestToOldestProperty =
            DependencyProperty.Register(nameof(IsNewestToOldest), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(true, OnIsNewestToOldestPropertyChanged));

        private static void OnIsNewestToOldestPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mailFilter = d as RSMailFilter;
            if (!e.NewValue.ToBool() || mailFilter == null)
            {
                return;
            }
            mailFilter.IsOldestToNewest = false;
            mailFilter.IsSmallToLarge = false;
            mailFilter.IsLargeToSmall = false;
            mailFilter.GetMailFilterModel().MailSortType = MailSortType.NewestToOldest;
            mailFilter.HandleDateSortPropertyChanged();
        }

        public bool IsOldestToNewest
        {
            get { return (bool)GetValue(IsOldestToNewestProperty); }
            set { SetValue(IsOldestToNewestProperty, value); }
        }

        public static readonly DependencyProperty IsOldestToNewestProperty =
            DependencyProperty.Register(nameof(IsOldestToNewest), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false, OnIsOldestToNewestPropertyChanged));

        private static void OnIsOldestToNewestPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mailFilter = d as RSMailFilter;
            if (!e.NewValue.ToBool() || mailFilter == null)
            {
                return;
            }

            mailFilter.IsNewestToOldest = false;
            mailFilter.IsSmallToLarge = false;
            mailFilter.IsLargeToSmall = false;
            mailFilter.GetMailFilterModel().MailSortType = MailSortType.OldestToNewest;
            mailFilter.HandleDateSortPropertyChanged();
        }


        public bool IsLargeToSmall
        {
            get { return (bool)GetValue(IsLargeToSmallProperty); }
            set { SetValue(IsLargeToSmallProperty, value); }
        }

        public static readonly DependencyProperty IsLargeToSmallProperty =
            DependencyProperty.Register(nameof(IsLargeToSmall), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false, OnIsLargeToSmallPropertyChanged));

        private static void OnIsLargeToSmallPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mailFilter = d as RSMailFilter;
            if (!e.NewValue.ToBool() || mailFilter == null)
            {
                return;
            }
            mailFilter.IsNewestToOldest = false;
            mailFilter.IsSmallToLarge = false;
            mailFilter.IsOldestToNewest = false;
            mailFilter.GetMailFilterModel().MailSortType = MailSortType.LargeToSmall;
            mailFilter.HandleSizeSortPropertyChanged();
        }

        public bool IsSmallToLarge
        {
            get { return (bool)GetValue(IsSmallToLargeProperty); }
            set { SetValue(IsSmallToLargeProperty, value); }
        }

        public static readonly DependencyProperty IsSmallToLargeProperty =
            DependencyProperty.Register(nameof(IsSmallToLarge), typeof(bool), typeof(RSMailFilter), new PropertyMetadata(false, OnIsSmallToLargePropertyChanged));

        private static void OnIsSmallToLargePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mailFilter = d as RSMailFilter;
            if (!e.NewValue.ToBool() || mailFilter == null)
            {
                return;
            }
            mailFilter.IsNewestToOldest = false;
            mailFilter.IsLargeToSmall = false;
            mailFilter.IsOldestToNewest = false;
            mailFilter.GetMailFilterModel().MailSortType = MailSortType.SmallToLarge;
            mailFilter.HandleSizeSortPropertyChanged();
        }

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
            this.ParentWindow = Window.GetWindow(this);
        }

        private void PART_BtnClear_Click(object sender, RoutedEventArgs e)
        {
            var mailFilterModel = GetMailFilterModel();
            mailFilterModel.MailFilterType = MailFilterType.AllRead;
            mailFilterModel.MailSortType = MailSortType.NewestToOldest;
            this.IsAllRead = true;
            this.IsNewestToOldest = true;
            this.GenerateMailFilterDes();
            this.RaiseMailFilterEvent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Popup = this.GetTemplateChild(nameof(this.PART_Popup)) as Popup;
            this.PART_MailSize = this.GetTemplateChild(nameof(this.PART_MailSize)) as RSDropdown;
            this.PART_MailDate = this.GetTemplateChild(nameof(this.PART_MailDate)) as RSDropdown;
        }

        private void HandleFilterPropertyChanged()
        {
            this.GenerateMailFilterDes();
            this.HidePopup();
            this.RaiseMailFilterEvent();
        }

        private void HandleSizeSortPropertyChanged()
        {
            this.GenerateMailFilterDes();
            this.HideMailSizePopup();
            this.HidePopup();
            RaiseMailFilterEvent();
        }

        private void HandleDateSortPropertyChanged()
        {
            this.GenerateMailFilterDes();
            this.HideMailDatePopup();
            this.HidePopup();
            RaiseMailFilterEvent();
        }

        private void HidePopup()
        {
            if (this.PART_Popup == null)
            {
                return;
            }

            this.PART_Popup.SetCurrentValue(Popup.IsOpenProperty, false);
            this.ActivateWindow();
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

        private void HideMailSizePopup()
        {
            if (this.PART_MailSize == null)
            {
                return;
            }

            this.PART_MailSize.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
            this.ActivateWindow();
        }

        private void HideMailDatePopup()
        {
            if (this.PART_MailDate == null)
            {
                return;
            }

            this.PART_MailDate.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
            this.ActivateWindow();
        }

        private void ActivateWindow()
        {
            if (this.ParentWindow == null)
            {
                return;
            }
            this.ParentWindow.Activate();
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
