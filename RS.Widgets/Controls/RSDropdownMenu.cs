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



        public double ArrowAngle
        {
            get { return (double)GetValue(ArrowAngleProperty); }
            set { SetValue(ArrowAngleProperty, value); }
        }

        public static readonly DependencyProperty ArrowAngleProperty =
            DependencyProperty.Register(nameof(ArrowAngle), typeof(double), typeof(RSDropdownMenu), new PropertyMetadata(90D));

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


        private void InitializeContextMenu()
        {
            if (this.InternalContextMenu != null)
            {
                return;
            }

            this.InternalContextMenu = new ContextMenu();

            if (this.Items != null)
            {
                foreach (var item in this.Items)
                {
                    this.InternalContextMenu.Items.Add(item);
                }
            }

            this.InternalContextMenu.Closed += this.InternalContextMenu_Closed;
        }


        private void InternalContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            this.SetCurrentValue(IsDropdownOpenProperty, false);
        }

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


        private void CloseContextMenu()
        {
            if (this.InternalContextMenu != null)
            {
                this.InternalContextMenu.IsOpen = false;
            }
        }

        #endregion

        /// <summary>
        /// 添加项到菜单
        /// </summary>
        public void AddItem(object item)
        {
            if (item == null || this.Items.Contains(item))
            {
                return;
            }
            
            this.Items.Add(item);
            this.InternalContextMenu?.Items.Add(item);
        }

        /// <summary>
        /// 从菜单移除项
        /// </summary>
        public void RemoveItem(object item)
        {
            this.InternalContextMenu?.Items.Remove(item);
            this.Items?.Remove(item);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_ToggleButton = this.GetTemplateChild(nameof(this.PART_ToggleButton)) as ToggleButton;
        }
    }
}
