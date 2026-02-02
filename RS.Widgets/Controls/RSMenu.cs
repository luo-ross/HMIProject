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
            // 延迟检查溢出，确保布局完成
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
                
                // 计算可用宽度（Menu总宽度减去溢出按钮宽度）
                double availableWidth = this.ActualWidth - overflowButtonWidth;

                // 第一步：找到第一个不可见的 MenuItem 索引
                int overflowIndex = this.Items.Count; // 默认为总数，表示无溢出
                double currentWidth = 0;

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

                // 第二步：从溢出索引开始，将所有项移到 DropdownMenu
                // 如果 overflowIndex == Items.Count，循环不执行
                for (int i = overflowIndex; i < this.Items.Count;)
                {
                    var item = this.Items[i];
                    if (item is MenuItem menuItem)
                    {
                        this.Items.Remove(menuItem);
                        this.PART_OverflowMenu.AddItem(menuItem);
                        // 设置 DropdownMenu 样式
                        this.SetMenuItemStyleForDropdown(menuItem);
                        // 注意：RemoveAt 后不需要 i++，因为后续元素会前移
                    }
                    else
                    {
                        i++; // 非 MenuItem，跳过
                    }
                }

                // 第三步：循环 DropdownMenu.Items（从前往后），检查是否可以移回 Menu
                for (int i = 0; i < this.PART_OverflowMenu.Items.Count;)
                {
                    var item = this.PART_OverflowMenu.Items[i];
                    if (item is MenuItem menuItem)
                    {
                        // 先临时设置为 Menu 样式以便测量
                        this.SetMenuItemStyleForMenu(menuItem);

                        // 测量这个 MenuItem
                        menuItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        double itemWidth = menuItem.DesiredSize.Width;

                        // 计算当前 Menu 的已用宽度
                        double currentMenuWidth = this.GetMenuItemsWidth();

                        // 如果加上这个item后不会超出可用宽度，则移回 Menu
                        if (currentMenuWidth + itemWidth <= availableWidth)
                        {
                            this.PART_OverflowMenu.RemoveItem(menuItem);
                            this.Items.Add(menuItem);
                            // Menu 样式已经在上面设置过了，保持不变
                            // 注意：RemoveAt 后不需要 i++
                        }
                        else
                        {
                            // 不能移回，恢复 DropdownMenu 样式
                            this.SetMenuItemStyleForDropdown(menuItem);
                            // 后面的项也肯定放不下，直接退出
                            break;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }

                // 更新溢出菜单的可见性
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
