using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace RS.Widgets.Controls
{
  
    [ContentProperty(nameof(Items))]
    public class RSDropdownMenu : ToggleButton
    {
        private ContextMenu InternalContextMenu;

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
                this.ShowContextMenu();
            }
            else
            {
                this.CloseContextMenu();
            }
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

            this.InternalContextMenu.PlacementTarget = this;
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
            this.SetCurrentValue(IsCheckedProperty, false);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
