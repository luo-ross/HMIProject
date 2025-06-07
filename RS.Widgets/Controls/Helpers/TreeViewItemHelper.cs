using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// 该方法来源于项目 iNKORE.UI.WPF.Modern 需要更多参看请参考下面的连接
    /// https://github.com/iNKORE-NET/UI.WPF.Modern
    /// </summary>
    public class TreeViewItemHelper
    {
        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(TreeViewItemHelper),
                new PropertyMetadata(OnIsEnabledChanged));

        public static bool GetIsEnabled(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeViewItem = (TreeViewItem)d;
            if ((bool)e.NewValue)
            {
                treeViewItem.IsVisibleChanged += OnTreeViewItemIsVisibleChanged;
                if (treeViewItem.IsVisible)
                {
                    UpdateIndentation(treeViewItem);
                }
            }
            else
            {
                treeViewItem.IsVisibleChanged -= OnTreeViewItemIsVisibleChanged;
                treeViewItem.ClearValue(IndentationPropertyKey);
            }
        }

        private static void OnTreeViewItemIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                UpdateIndentation((TreeViewItem)sender);
            }
        }

        #endregion


        #region Indentation

        private static readonly DependencyPropertyKey IndentationPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "Indentation",
                typeof(Thickness),
                typeof(TreeViewItemHelper),
                null);

        /// <summary>
        /// Identifies the Indentation dependency property.
        /// </summary>
        public static readonly DependencyProperty IndentationProperty =
            IndentationPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the amount that the item is indented.
        /// </summary>
        /// <param name="treeViewItem">The element from which to read the property value.</param>
        /// <returns>The amount that the item is indented.</returns>
        public static Thickness GetIndentation(TreeViewItem treeViewItem)
        {
            return (Thickness)treeViewItem.GetValue(IndentationProperty);
        }

        public static void SetIndentation(TreeViewItem treeViewItem, Thickness value)
        {
            treeViewItem.SetValue(IndentationPropertyKey, value);
        }

        private static void UpdateIndentation(TreeViewItem item)
        {
            SetIndentation(item, new Thickness(GetDepth(item) * 16, 0, 0, 0));
        }

        #endregion

        private static int GetDepth(TreeViewItem item)
        {
            int depth = 0;
            while (ItemsControl.ItemsControlFromItemContainer(item) is TreeViewItem parentItem)
            {
                depth++;
                item = parentItem;
            }
            return depth;
        }
    }
}
