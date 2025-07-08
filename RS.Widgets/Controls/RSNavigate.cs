using CommunityToolkit.Mvvm.Input;
using IdGen;
using NPOI.POIFS.Properties;
using RS.Commons;
using RS.Widgets.Commons;
using RS.Widgets.Converters;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using RS.Win32API;
using ScottPlot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using ZXing;

namespace RS.Widgets.Controls
{
    public class RSNavigate : ContentControl, INavigate
    {
        public RSWindow ParentWin { get; set; }
        private RSSearch PART_Search;
        private Border PART_NavHost;
        private IDialog PART_Dialog;
        private double CurrenNavWidth;
        private ListBox PART_NavList;
        private Frame PART_Frame;

        public static readonly RoutedEvent NavItemClickEvent = EventManager.RegisterRoutedEvent(
            nameof(NavItemClick),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(RSNavigate));

        public static readonly RoutedEvent NavItemDoubleClickEvent = EventManager.RegisterRoutedEvent(
           nameof(NavItemDoubleClick),
           RoutingStrategy.Bubble,
           typeof(RoutedEventHandler),
           typeof(RSNavigate));


        public event RoutedEventHandler NavItemClick
        {
            add { AddHandler(NavItemClickEvent, value); }
            remove { RemoveHandler(NavItemClickEvent, value); }
        }

        public event RoutedEventHandler NavItemDoubleClick
        {
            add { AddHandler(NavItemDoubleClickEvent, value); }
            remove { RemoveHandler(NavItemDoubleClickEvent, value); }
        }

        public static readonly DependencyProperty NavItemClickCommandProperty =
            DependencyProperty.Register(nameof(NavItemClickCommand),
            typeof(RelayCommand<NavigateModel>),
            typeof(RSNavigate),
            new PropertyMetadata(null));

        public RelayCommand<NavigateModel> NavItemClickCommand
        {
            get { return (RelayCommand<NavigateModel>)GetValue(NavItemClickCommandProperty); }
            set { SetValue(NavItemClickCommandProperty, value); }
        }

        public static readonly DependencyProperty NavItemDoubleClickCommandProperty =
            DependencyProperty.Register(nameof(NavItemDoubleClickCommand),
            typeof(RelayCommand<NavigateModel>),
            typeof(RSNavigate),
            new PropertyMetadata(null));

        public RelayCommand<NavigateModel> NavItemDoubleClickCommand
        {
            get { return (RelayCommand<NavigateModel>)GetValue(NavItemDoubleClickCommandProperty); }
            set { SetValue(NavItemDoubleClickCommandProperty, value); }
        }

        static RSNavigate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSNavigate), new FrameworkPropertyMetadata(typeof(RSNavigate)));
        }

        public RSNavigate()
        {
            this.Unloaded += RSNavigate_Unloaded;
            this.DataContextChanged += RSNavigate_DataContextChanged;
        }

        private void RSNavigate_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            NavigateHelper.UnregisterNavigate(e.OldValue);
            NavigateHelper.RegisterNavigate(e.NewValue, this);
        }

        private void RSNavigate_Unloaded(object sender, RoutedEventArgs e)
        {
            NavigateHelper.UnregisterNavigate(this.DataContext);
        }

        #region 依赖属性

        public double NavCollapsedMinWidth
        {
            get { return (double)GetValue(NavCollapsedMinWidthProperty); }
            set { SetValue(NavCollapsedMinWidthProperty, value); }
        }

        public static readonly DependencyProperty NavCollapsedMinWidthProperty =
            DependencyProperty.Register("NavCollapsedMinWidth", typeof(double), typeof(RSNavigate), new PropertyMetadata(50D, OnNavCollapsedMinWidthPropertyChanged));

        private static void OnNavCollapsedMinWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
        }

        public double NavExpandMinWidth
        {
            get { return (double)GetValue(NavExpandMinWidthProperty); }
            set { SetValue(NavExpandMinWidthProperty, value); }
        }

        public static readonly DependencyProperty NavExpandMinWidthProperty =
            DependencyProperty.Register("NavExpandMinWidth", typeof(double), typeof(RSNavigate), new PropertyMetadata(280D));



        [Browsable(false)]
        public ScreenSize ScreenSize
        {
            get { return (ScreenSize)GetValue(ScreenSizeProperty); }
            set { SetValue(ScreenSizeProperty, value); }
        }

        public static readonly DependencyProperty ScreenSizeProperty =
            DependencyProperty.Register("ScreenSize", typeof(ScreenSize), typeof(RSNavigate), new PropertyMetadata(ScreenSize.Unknown, OnScreenSizePropertyChanged));

        private static void OnScreenSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
        }

        [Browsable(false)]
        public NavStatus NavStatus
        {
            get { return (NavStatus)GetValue(NavStatusProperty); }
            set { SetValue(NavStatusProperty, value); }
        }

        public static readonly DependencyProperty NavStatusProperty =
            DependencyProperty.Register("NavStatus", typeof(NavStatus), typeof(RSNavigate), new PropertyMetadata(NavStatus.Unknown, OnNavStatusPropertyChanged));

        private static void OnNavStatusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
        }

        [Browsable(false)]
        public NavType NavType
        {
            get { return (NavType)GetValue(NavTypeProperty); }
            set { SetValue(NavTypeProperty, value); }
        }

        public static readonly DependencyProperty NavTypeProperty =
            DependencyProperty.Register("NavType", typeof(NavType), typeof(RSNavigate), new PropertyMetadata(NavType.Unknown, OnNavTypePropertyChanged));

        private static void OnNavTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
        }

        [Browsable(false)]
        public bool IsNavExpanded
        {
            get { return (bool)GetValue(IsNavExpandedProperty); }
            set { SetValue(IsNavExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsNavExpandedProperty =
            DependencyProperty.Register("IsNavExpanded", typeof(bool), typeof(RSNavigate), new PropertyMetadata(true, OnIsNavExpandedPropertyChanged));

        private static void OnIsNavExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpdateNavigateModelList();
        }


        public List<NavigateModel> ItemsSource
        {
            get { return (List<NavigateModel>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(List<NavigateModel>), typeof(RSNavigate), new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = d as RSNavigate;
            rsNavigate.UpdateNavigateModelList();
        }

        public ObservableCollection<NavigateModel> NavigateModelList
        {
            get { return (ObservableCollection<NavigateModel>)GetValue(NavigateModelListProperty); }
            private set { SetValue(NavigateModelListProperty, value); }
        }

        public static readonly DependencyProperty NavigateModelListProperty =
            DependencyProperty.Register("NavigateModelList", typeof(ObservableCollection<NavigateModel>), typeof(RSNavigate), new PropertyMetadata(null));


        public NavigateModel NavigateModelSelect
        {
            get { return (NavigateModel)GetValue(NavigateModelSelectProperty); }
            set { SetValue(NavigateModelSelectProperty, value); }
        }

        public static readonly DependencyProperty NavigateModelSelectProperty =
            DependencyProperty.Register("NavigateModelSelect", typeof(NavigateModel), typeof(RSNavigate), new PropertyMetadata(null));


        public object FooterContent
        {
            get { return (object)GetValue(FooterContentProperty); }
            set { SetValue(FooterContentProperty, value); }
        }

        public static readonly DependencyProperty FooterContentProperty =
            DependencyProperty.Register("FooterContent", typeof(object), typeof(RSNavigate), new PropertyMetadata(null));




        public string ViewKey
        {
            get { return (string)GetValue(ViewKeyProperty); }
            set { SetValue(ViewKeyProperty, value); }
        }

        public static readonly DependencyProperty ViewKeyProperty =
            DependencyProperty.Register("ViewKey", typeof(string), typeof(RSNavigate), new PropertyMetadata(null, OnViewKeyPropertyChanged));
        private static void OnViewKeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = d as RSNavigate;
            rsNavigate.UpdateView();

        }
        #endregion





        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.ParentWin = this.TryFindParent<RSWindow>();
            if (this.ParentWin != null)
            {
                this.ParentWin.SizeChanged -= ParentWin_SizeChanged;
                this.ParentWin.SizeChanged += ParentWin_SizeChanged;
            }


            this.PART_Search = this.GetTemplateChild(nameof(this.PART_Search)) as RSSearch;
            this.PART_NavHost = this.GetTemplateChild(nameof(this.PART_NavHost)) as Border;
            this.PART_Dialog = this.GetTemplateChild(nameof(this.PART_Dialog)) as RSDialog;
            this.PART_NavList = this.GetTemplateChild(nameof(this.PART_NavList)) as ListBox;
            this.PART_Frame = this.GetTemplateChild(nameof(this.PART_Frame)) as Frame;
            if (this.PART_Search != null)
            {
                this.PART_Search.OnBtnSearchCallBack -= PART_Search_OnBtnSearchCallBack;
                this.PART_Search.OnBtnSearchCallBack += PART_Search_OnBtnSearchCallBack;
            }


            this.UpdateNavType();
        }



        // 获取ScrollViewer的通用方法
        public ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer) return (ScrollViewer)depObj;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }


        internal void UpdateNavigateModelSelect(NavigateModel? model)
        {
            //获取到选择项
            if (model != null)
            {
                foreach (var item in this.ItemsSource)
                {
                    item.IsSelect = false;
                }
                model.IsSelect = true;
            }

            this.NavigateModelSelect = model;
        }

        internal void UpdateNavigateModelList()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var dataList = this.ItemsSource.Where(t => string.IsNullOrEmpty(t.ParentId)).ToList();
            var copyList = dataList.ToList();

            foreach (var item in copyList)
            {
                if (item.IsExpand)
                {
                    var index = dataList.IndexOf(item);
                    var childList = GetChildList(item.Id);
                    dataList.InsertRange(index + 1, childList);
                }
            }

            this.NavigateModelList = new ObservableCollection<NavigateModel>(dataList);
            Debug.WriteLine(stopwatch.ElapsedMilliseconds);
        }


        public List<NavigateModel> GetChildList(string parentId)
        {
            List<NavigateModel> childList = this.ItemsSource.Where(t => t.ParentId == parentId && !string.IsNullOrEmpty(parentId)).ToList();
            var copyList= childList.ToList();
            foreach (var item in copyList)
            {
                if (item.IsExpand)
                {
                    var childern = this.GetChildList(item.Id);
                    if (childern.Count > 0)
                    {
                        var index = childList.IndexOf(item);
                        childList.InsertRange(index + 1, childern);
                    }
                }
            }
            return childList;
        }

        #region 内部方法
        private void ParentWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateNavType();
        }

        private void UpdateNavType()
        {
            if (this.ParentWin == null)
            {
                return;
            }

            var width = this.ParentWin.Width;

            //尺寸小于1000的时候显示迷你导航
            //尺寸小于650 隐藏迷你导航
            //尺寸在1000以内时 导航展开就是浮动导航
            //尺寸在1000以上  导航展开就是宽度自适应
            //展开的时候根据屏幕尺寸判断是浮动还是宽度自适应
            //缩小的时候根据屏幕判断是显示迷你版导航还是隐藏导航
            if (width <= 650)
            {
                this.ScreenSize = ScreenSize.Small;
                this.NavType = NavType.Floating;
            }
            else if (width <= 1000)
            {
                this.ScreenSize = ScreenSize.Medium;
                this.NavType = NavType.Floating;
            }
            else
            {
                this.ScreenSize = ScreenSize.Large;
                this.NavType = NavType.AdaptiveWidth;
            }
        }



        private void UpdateView()
        {
            if (this.NavigateModelSelect == null)
            {
                return;
            }
            this.SetResourceReference(RSNavigate.ContentProperty, this.NavigateModelSelect.ViewKey);
        }


        private void PART_Search_OnBtnSearchCallBack(string search)
        {
            if (!this.IsNavExpanded)
            {
                this.IsNavExpanded = true;
            }
            if (!string.IsNullOrEmpty(search))
            {
                var searchResult = this.ItemsSource
                    .Where(t => t.NavName.Contains(search)
                    && !t.IsGroupNav).ToList();
                this.NavigateModelList = new ObservableCollection<NavigateModel>(searchResult);
            }
            else
            {
                this.UpdateNavigateModelList();
            }
        }

        private void ExpandParentNav(NavigateModel navigateModel)
        {
            var parent = this.ItemsSource
                .Where(t => t.Id == navigateModel.ParentId).FirstOrDefault();
            if (parent != null)
            {
                parent.IsExpand = true;
                this.ExpandParentNav(parent);
            }
        }
        #endregion


        #region Interface implementation
        public Task<OperateResult> InvokeAsync(Func<CancellationToken,
            Task<OperateResult>> func,
            LoadingConfig loadingConfig = null,
            CancellationToken cancellationToken = default)
        {
            return this.Loading.InvokeAsync(func, loadingConfig, cancellationToken);
        }

        public Task<OperateResult<T>> InvokeAsync<T>(Func<CancellationToken,
            Task<OperateResult<T>>> func,
            LoadingConfig loadingConfig = null,
            CancellationToken cancellationToken = default)
        {
            return this.Loading.InvokeAsync(func, loadingConfig, cancellationToken);
        }

        void IModal.ShowModal(object content)
        {
            this.Modal.ShowModal(content);
        }

        void IModal.CloseModal()
        {
            this.Modal.CloseModal();
        }

        void IWinModal.ShowModal(object content)
        {
            this.WinModal.ShowModal(content);
        }

        void IWinModal.CloseModal()
        {
            this.WinModal.CloseModal();
        }


        public void ShowDialog(object content)
        {
            this.WinModal.ShowDialog(content);
        }

        public void HandleBtnClickEvent()
        {
            this.MessageBox.HandleBtnClickEvent();
        }

        public void MessageBoxDisplay(Window window)
        {
            this.MessageBox.MessageBoxDisplay(window);
        }

        public void MessageBoxClose()
        {
            this.MessageBox.MessageBoxClose();
        }

        public async Task<MessageBoxResult> ShowMessageAsync(
            Window window,
            string messageBoxText = null,
            string caption = null,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            MessageBoxOptions options = MessageBoxOptions.None)
        {
            return await this.MessageBox.ShowMessageAsync(window,
                messageBoxText,
                caption,
                button,
                icon,
                defaultResult,
                options);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
            string caption,
            MessageBoxButton button)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button, icon);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText,
                caption,
                button,
                icon,
                defaultResult);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult,
            MessageBoxOptions options)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText,
                caption,
                button,
                icon,
                defaultResult,
                options);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window,
            string messageBoxText,
            string caption)
        {
            return await this.MessageBox.ShowMessageAsync(window,
                messageBoxText,
                caption);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window,
            string messageBoxText,
            string caption,
            MessageBoxButton button)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window,
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon)
        {
            return await this.MessageBox.ShowMessageAsync(window,
                messageBoxText,
                caption,
                button,
                icon);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(
            Window window,
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult)
        {
            return await this.MessageBox.ShowMessageAsync(window,
                messageBoxText,
                caption, button,
                icon,
                defaultResult);
        }


        IWindow IDialog.ParentWin
        {
            get
            {
                return this.ParentWin;
            }
        }

        INavigate IDialog.Navigate
        {
            get
            {
                return this;
            }
        }

        public ILoading Loading
        {
            get
            {
                return this.PART_Dialog?.Loading;
            }
        }

        public ILoading RootLoading
        {
            get
            {
                return this.ParentWin?.Loading;
            }
        }

        public IModal Modal
        {
            get
            {
                return this.PART_Dialog?.Modal;
            }
        }

        public IModal RootModal
        {
            get
            {
                return this.ParentWin?.Modal;
            }
        }

        public IMessage MessageBox
        {
            get
            {
                return this.PART_Dialog?.MessageBox;
            }
        }

        public IMessage RootMessageBox
        {
            get
            {
                return this.ParentWin.MessageBox;
            }
        }


        public IWinModal WinModal
        {
            get
            {
                return new RSWinModal(this.ParentWin as Window);
            }
        }

        public IWinMessage WinMessageBox
        {
            get
            {
                return new RSWinMessage(this.ParentWin as Window);
            }
        }






        #endregion

    }
}
