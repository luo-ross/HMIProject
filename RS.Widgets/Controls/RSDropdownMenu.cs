using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace RS.Widgets.Controls
{
    [ContentProperty(nameof(Items))]
    public class RSDropdownMenu : ContentControl
    {
        private ContextMenu InternalContextMenu;
        private ToggleButton PART_ToggleButton;

        static RSDropdownMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDropdownMenu), new FrameworkPropertyMetadata(typeof(RSDropdownMenu)));
        }

        public RSDropdownMenu()
        {
            this.Items = new ObservableCollection<object>();
            this.Loaded += this.RSDropdownMenu_Loaded;
            this.Unloaded += this.RSDropdownMenu_Unloaded;
        }

        #region 依赖属性

        /// <summary>
        /// 是否展开下拉菜单
        /// </summary>
        public bool IsDropdownOpen
        {
            get { return (bool)GetValue(IsDropdownOpenProperty); }
            set { SetValue(IsDropdownOpenProperty, value); }
        }

        public static readonly DependencyProperty IsDropdownOpenProperty =
            DependencyProperty.Register(nameof(IsDropdownOpen), typeof(bool), typeof(RSDropdownMenu), new PropertyMetadata(false, OnIsDropdownOpenChanged));

        private static void OnIsDropdownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RSDropdownMenu dropdownMenu = (RSDropdownMenu)d;
            bool isOpen = (bool)e.NewValue;

            if (isOpen)
            {
                dropdownMenu.ShowContextMenu();
            }
            else
            {
                dropdownMenu.CloseContextMenu();
            }
        }


        /// <summary>
        /// 菜单子项集合（支持 MenuItem、Separator 等）
        /// </summary>
        public ObservableCollection<object> Items
        {
            get { return (ObservableCollection<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<object>), typeof(RSDropdownMenu), new PropertyMetadata(null));


        /// <summary>
        /// 菜单放置位置
        /// </summary>
        public PlacementMode Placement
        {
            get { return (PlacementMode)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(RSDropdownMenu), new PropertyMetadata(PlacementMode.Bottom));


        /// <summary>
        /// 是否显示下拉箭头图标
        /// </summary>
        public bool IsShowDropDownIcon
        {
            get { return (bool)GetValue(IsShowDropDownIconProperty); }
            set { SetValue(IsShowDropDownIconProperty, value); }
        }

        public static readonly DependencyProperty IsShowDropDownIconProperty =
            DependencyProperty.Register(nameof(IsShowDropDownIcon), typeof(bool), typeof(RSDropdownMenu), new PropertyMetadata(true));

        #endregion

        #region 事件处理

        private void RSDropdownMenu_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeContextMenu();
        }

        private void RSDropdownMenu_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.InternalContextMenu != null)
            {
                this.InternalContextMenu.Closed -= this.InternalContextMenu_Closed;
            }
        }

        /// <summary>
        /// 初始化内部 ContextMenu
        /// </summary>
        private void InitializeContextMenu()
        {
            if (this.InternalContextMenu != null)
            {
                return;
            }

            this.InternalContextMenu = new ContextMenu();

            // 将 Items 中的 MenuItem 添加到 ContextMenu
            if (this.Items != null)
            {
                foreach (var item in this.Items)
                {
                    this.InternalContextMenu.Items.Add(item);
                }
            }

            // 订阅 ContextMenu 关闭事件
            this.InternalContextMenu.Closed += this.InternalContextMenu_Closed;
        }

        /// <summary>
        /// ContextMenu 关闭事件处理
        /// </summary>
        private void InternalContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            // 菜单关闭时，重置 IsDropdownOpen 状态
            this.SetCurrentValue(IsDropdownOpenProperty, false);
        }

        /// <summary>
        /// 显示 ContextMenu
        /// </summary>
        private void ShowContextMenu()
        {
            if (this.InternalContextMenu == null)
            {
                return;
            }

            // 使用 ToggleButton 作为放置目标
            this.InternalContextMenu.PlacementTarget = this.PART_ToggleButton != null ? (UIElement)this.PART_ToggleButton : this;
            this.InternalContextMenu.Placement = this.Placement;
            this.InternalContextMenu.IsOpen = true;
        }

        /// <summary>
        /// 关闭 ContextMenu
        /// </summary>
        private void CloseContextMenu()
        {
            if (this.InternalContextMenu != null)
            {
                this.InternalContextMenu.IsOpen = false;
            }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_ToggleButton = this.GetTemplateChild(nameof(this.PART_ToggleButton)) as ToggleButton;
        }
    }
}
