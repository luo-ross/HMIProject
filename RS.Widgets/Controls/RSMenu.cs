using System;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    public class RSMenu : Menu
    {
        private RSDropdownMenu PART_OverflowMenu;
        private bool IsUpdatingOverflow = false;

        static RSMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSMenu), new FrameworkPropertyMetadata(typeof(RSMenu)));
        }

        public RSMenu()
        {
            this.Loaded += this.RSMenu_Loaded;
            this.SizeChanged += this.RSMenu_SizeChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_OverflowMenu = this.GetTemplateChild(nameof(this.PART_OverflowMenu)) as RSDropdownMenu;
        }

        private void RSMenu_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.CheckForOverflow();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void RSMenu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                this.CheckForOverflow();
            }
        }

        /// <summary>
        /// 检查溢出并重新分配 MenuItems
        /// </summary>
        private void CheckForOverflow()
        {
            if (this.PART_OverflowMenu == null || this.IsUpdatingOverflow)
            {
                return;
            }

            this.IsUpdatingOverflow = true;

            try
            {
                // 测量溢出按钮的实际宽度
                this.PART_OverflowMenu.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double overflowButtonWidth = this.PART_OverflowMenu.DesiredSize.Width;
                
                // 计算可用宽度
                double availableWidth = this.ActualWidth - overflowButtonWidth;

                // 第一步：计算当前主菜单需要的宽度，找到溢出点
                double currentWidth = 0;
                int overflowIndex = this.Items.Count;

                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i] is MenuItem menuItem)
                    {
                        menuItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        currentWidth += menuItem.DesiredSize.Width;

                        if (currentWidth > availableWidth)
                        {
                            overflowIndex = i;
                            break;
                        }
                    }
                }

                // 第二步：从后往前移除超出的项，插入到溢出菜单开头
                // 这样溢出菜单中的顺序与原始顺序一致
                while (this.Items.Count > overflowIndex)
                {
                    int lastIndex = this.Items.Count - 1;
                    var item = this.Items[lastIndex];
                    if (item is MenuItem menuItem)
                    {
                        this.Items.RemoveAt(lastIndex);
                        // 插入到溢出菜单开头（index 0）
                        this.PART_OverflowMenu.InsertItem(0, menuItem);
                        this.SetMenuItemStyleForDropdown(menuItem);
                    }
                }

                // 第三步：尝试将溢出菜单中的项移回主菜单
                // 从溢出菜单开头取出，添加到主菜单末尾
                while (this.PART_OverflowMenu.Items.Count > 0)
                {
                    if (this.PART_OverflowMenu.Items[0] is MenuItem menuItem)
                    {
                        // 先设置为 Menu 样式以便测量
                        this.SetMenuItemStyleForMenu(menuItem);

                        menuItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        double itemWidth = menuItem.DesiredSize.Width;
                        double currentMenuWidth = this.GetMenuItemsWidth();

                        if (currentMenuWidth + itemWidth <= availableWidth)
                        {
                            this.PART_OverflowMenu.RemoveItem(menuItem);
                            // 添加到主菜单末尾
                            this.Items.Add(menuItem);
                        }
                        else
                        {
                            // 恢复样式并退出
                            this.SetMenuItemStyleForDropdown(menuItem);
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                this.UpdateOverflowMenuVisibility();
            }
            finally
            {
                this.IsUpdatingOverflow = false;
            }
        }

        /// <summary>
        /// 设置 MenuItem 为 Menu 样式
        /// </summary>
        private void SetMenuItemStyleForMenu(MenuItem menuItem)
        {
            menuItem.HorizontalContentAlignment = HorizontalAlignment.Center;
            menuItem.VerticalContentAlignment = VerticalAlignment.Center;
            menuItem.BorderThickness = new Thickness(1);
            ControlsHelper.SetIconRotateAngle(menuItem, 90);
        }

        /// <summary>
        /// 设置 MenuItem 为 DropdownMenu 样式
        /// </summary>
        private void SetMenuItemStyleForDropdown(MenuItem menuItem)
        {
            menuItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            menuItem.VerticalContentAlignment = VerticalAlignment.Stretch;
            menuItem.BorderThickness = new Thickness(0);
            ControlsHelper.SetIconRotateAngle(menuItem, 0);
        }

        /// <summary>
        /// 计算 Menu 中所有 MenuItem 的总宽度
        /// </summary>
        private double GetMenuItemsWidth()
        {
            double totalWidth = 0;
            foreach (var item in this.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    totalWidth += menuItem.DesiredSize.Width;
                }
            }
            return totalWidth;
        }

        /// <summary>
        /// 更新溢出菜单的可见性
        /// </summary>
        private void UpdateOverflowMenuVisibility()
        {
            if (this.PART_OverflowMenu != null)
            {
                this.PART_OverflowMenu.Visibility = this.PART_OverflowMenu.Items.Count > 0
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }
    }
}
