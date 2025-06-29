using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Routing;
using RS.Commons;
using RS.Models;
using RS.Widgets.Commons;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace RS.Widgets.Controls
{
    public class RSNavigate : Frame, INavigate
    {
        public RSWindow ParentWin { get; set; }
        private RSSearch PART_Search;
        private Border PART_NavHost;
        private IDialog PART_Dialog;
        private double CurrenNavWidth = 0;
        private ListBox PART_NavList;


        public static readonly RoutedEvent NavItemClickEvent = EventManager.RegisterRoutedEvent(
            nameof(NavItemClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSNavigate));

        public static readonly RoutedEvent NavItemDoubleClickEvent = EventManager.RegisterRoutedEvent(
           nameof(NavItemDoubleClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSNavigate));


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
       DependencyProperty.Register(nameof(NavItemClickCommand), typeof(RelayCommand<NavigateModel>), typeof(RSNavigate), new PropertyMetadata(null));

        public RelayCommand<NavigateModel> NavItemClickCommand
        {
            get { return (RelayCommand<NavigateModel>)GetValue(NavItemClickCommandProperty); }
            set { SetValue(NavItemClickCommandProperty, value); }
        }

        public static readonly DependencyProperty NavItemDoubleClickCommandProperty =
    DependencyProperty.Register(nameof(NavItemDoubleClickCommand), typeof(RelayCommand<NavigateModel>), typeof(RSNavigate), new PropertyMetadata(null));

        public RelayCommand<NavigateModel> NavItemDoubleClickCommand
        {
            get { return (RelayCommand<NavigateModel>)GetValue(NavItemDoubleClickCommandProperty); }
            set { SetValue(NavItemDoubleClickCommandProperty, value); }
        }

        static RSNavigate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSNavigate), new FrameworkPropertyMetadata(typeof(RSNavigate)));
        }

        public ICommand NavItemInernalClickCommand { get;  }

        public RSNavigate()
        {
            this.Unloaded += RSNavigate_Unloaded;
            this.DataContextChanged += RSNavigate_DataContextChanged;
            this.NavItemInernalClickCommand = new RelayCommand<Tuple<object, MouseButtonEventArgs>>(OnNavItemClick);
        }

        private void OnNavItemClick(Tuple<object, MouseButtonEventArgs> parameter)
        {
            var model = parameter.Item1 as NavigateModel;
            var eventArgs = parameter.Item2;
            if (eventArgs.ClickCount == 2)
            {
                this.UpdateNavigateModelList();

                this.RaiseEvent(new RoutedEventArgs()
                {
                    RoutedEvent = NavItemDoubleClickEvent,
                    Source = model
                });
                if (this.NavItemDoubleClickCommand != null)
                {
                    this.NavItemDoubleClickCommand.Execute(model);
                }
            }
            else
            {
                this.UpdateNavigateModelSelect(model);

                this.RaiseEvent(new RoutedEventArgs()
                {
                    RoutedEvent = NavItemClickEvent,
                    Source = model
                });

                if (this.NavItemClickCommand != null)
                {
                   this.NavItemClickCommand.Execute(model);
                }
            }
        }

        private void UpdateNavigateModelSelect(NavigateModel? model)
        {
            //获取到选择项
            if (model != null)
            {
                foreach (var item in this.NavigateModelList)
                {
                    item.IsSelect = false;
                }
                model.IsSelect = true;
            }

            this.NavigateModelSelect = model;
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

        private void RSDialog_Loaded(object sender, RoutedEventArgs e)
        {

        }

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

        public double NavNormalWidth
        {
            get { return (double)GetValue(NavNormalWidthProperty); }
            set { SetValue(NavNormalWidthProperty, value); }
        }

        public static readonly DependencyProperty NavNormalWidthProperty =
            DependencyProperty.Register("NavNormalWidth", typeof(double), typeof(RSNavigate), new PropertyMetadata(280D, OnNavNormalWidthPropertyChanged));

        private static void OnNavNormalWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpateNavVisualState();
        }

        public double NavMinWidth
        {
            get { return (double)GetValue(NavMinWidthProperty); }
            set { SetValue(NavMinWidthProperty, value); }
        }

        public static readonly DependencyProperty NavMinWidthProperty =
            DependencyProperty.Register("NavMinWidth", typeof(double), typeof(RSNavigate), new PropertyMetadata(50D, OnNavMinWidthPropertyChanged));

        private static void OnNavMinWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpateNavVisualState();
        }

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
            rsNavigate.UpateNavVisualState();
        }

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
            rsNavigate.UpateNavVisualState();
        }

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
            rsNavigate.UpateNavVisualState();
        }

        public object BodyCotent
        {
            get { return (object)GetValue(BodyCotentProperty); }
            set { SetValue(BodyCotentProperty, value); }
        }

        public static readonly DependencyProperty BodyCotentProperty =
            DependencyProperty.Register("BodyCotent", typeof(object), typeof(RSNavigate), new PropertyMetadata(null));

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
            rsNavigate.UpateNavVisualState();
        }


        private void UpateNavVisualState()
        {
            //手动优先级最高
            if (this.IsNavExpanded)
            {
                this.NavStatus = NavStatus.Expanded;
            }
            else
            {
                this.NavStatus = NavStatus.Collapsed;
            }

            switch (this.ScreenSize)
            {
                case ScreenSize.Unknown:
                    return;
                case ScreenSize.Small:
                    if (this.NavStatus == NavStatus.Expanded)
                    {
                        AnimateNavHostWidth(this.NavNormalWidth, TimeSpan.FromSeconds(0.2));
                    }
                    else
                    {
                        AnimateNavHostWidth(0, TimeSpan.FromSeconds(0));
                    }
                    break;
                case ScreenSize.Medium:
                    if (this.NavStatus == NavStatus.Expanded)
                    {
                        AnimateNavHostWidth(this.NavNormalWidth, TimeSpan.FromSeconds(0.2));
                    }
                    else
                    {
                        AnimateNavHostWidth(this.NavMinWidth, TimeSpan.FromSeconds(0));
                    }
                    break;
                case ScreenSize.Large:
                    if (this.NavStatus == NavStatus.Expanded)
                    {
                        AnimateNavHostWidth(this.NavNormalWidth, TimeSpan.FromSeconds(0.2));
                    }
                    else
                    {
                        AnimateNavHostWidth(this.NavMinWidth, TimeSpan.FromSeconds(0.2));
                    }
                    break;
            }
      
        }


        private void AnimateNavHostWidth(double toWidth, TimeSpan duration)
        {
            if (this.CurrenNavWidth != toWidth)
            {
                var animation = new DoubleAnimation
                {
                    To = toWidth,
                    Duration = duration
                };
                this.PART_NavHost?.BeginAnimation(FrameworkElement.WidthProperty, animation);
            }

            this.CurrenNavWidth = toWidth;
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
            var dataList = rsNavigate.ItemsSource.Where(t => string.IsNullOrEmpty(t.ParentId)).ToList();
            rsNavigate.NavigateModelList = new ObservableCollection<NavigateModel>(dataList);
            rsNavigate.UpdateNavigateModelList();
        }

        private void UpdateNavigateModelList()
        {
            var current = this.NavigateModelSelect;
            if (current != null)
            {
                var dataList = this.ItemsSource.Where(t => t.ParentId == current.Id).ToList();
                this.NavigateModelList.RemoveRangeWithCascade(dataList, t => t.Id, t => t.ParentId);
                if (current.IsExpand)
                {
                    current.IsExpand = false;
                }
                else
                {
                    current.IsExpand = true;
                    var selectIndex = this.NavigateModelList.IndexOf(current);
                    this.NavigateModelList.InsertRange(selectIndex + 1, dataList);
                    //如果展开 要把子集已经展开的都展开
                    foreach (var item in dataList)
                    {
                        if (item.IsExpand)
                        {
                            NavigateExpand(current);
                        }
                    }
                }
            }
        }

        private void NavigateExpand(NavigateModel current)
        {
            if (current != null)
            {
                var dataList = this.ItemsSource.Where(t => t.ParentId == current.Id).ToList();
                this.NavigateModelList.RemoveRangeWithCascade(dataList, t => t.Id, t => t.ParentId);
                var selectIndex = this.NavigateModelList.IndexOf(current);
                this.NavigateModelList.InsertRange(selectIndex + 1, dataList);
                foreach (var item in dataList)
                {
                    if (item.IsExpand)
                    {
                        NavigateExpand(item);
                    }
                }
            }
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


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PART_Search = this.GetTemplateChild(nameof(this.PART_Search)) as RSSearch;
            this.PART_NavHost = this.GetTemplateChild(nameof(this.PART_NavHost)) as Border;
            this.PART_Dialog = this.GetTemplateChild(nameof(this.PART_Dialog)) as RSDialog;
            this.PART_NavList = this.GetTemplateChild(nameof(this.PART_NavList)) as ListBox;

            if (this.PART_Search != null)
            {
                this.PART_Search.OnBtnSearchCallBack -= PART_Search_OnBtnSearchCallBack;
                this.PART_Search.OnBtnSearchCallBack += PART_Search_OnBtnSearchCallBack;
            }

            this.ParentWin = this.TryFindParent<RSWindow>();

            if (this.ParentWin != null)
            {
                this.ParentWin.SizeChanged -= ParentWin_SizeChanged;
                this.ParentWin.SizeChanged += ParentWin_SizeChanged;
            }

            if (this.PART_NavList != null)
            {
                this.PART_NavList.PreviewKeyDown += PART_NavList_PreviewKeyDown;
            }

            this.UpdateNavType();
        }

        private void PART_NavList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //e.Handled = true;
        }

        private void PART_Search_OnBtnSearchCallBack(string obj)
        {
            if (!this.IsNavExpanded)
            {
                this.IsNavExpanded = true;
            }
        }


        #region Interface implementation

        public Task<OperateResult> InvokeAsync(Func<CancellationToken, Task<OperateResult>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        {
            return this.Loading.InvokeAsync(func, loadingConfig, cancellationToken);
        }

        public Task<OperateResult<T>> InvokeAsync<T>(Func<CancellationToken, Task<OperateResult<T>>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
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

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText = null, string caption = null, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button, icon, defaultResult, options);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button, icon);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button, icon, defaultResult);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button, icon, defaultResult, options);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText, string caption)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText, string caption, MessageBoxButton button)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button, icon);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button, icon, defaultResult);
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
