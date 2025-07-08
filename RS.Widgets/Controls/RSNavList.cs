using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.Widgets.Controls
{
    public class RSNavList : ListBox
    {
        /// <summary>
        /// 这个当行列表拥有的弹出层
        /// </summary>
        public RSNavPopup RSNavPopup { get; set; }

        public RSNavList()
        {
            this.Unloaded += RSNavList_Unloaded;
        }

        private void RSNavList_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RSNavItem();
        }

        private void RsNavItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
