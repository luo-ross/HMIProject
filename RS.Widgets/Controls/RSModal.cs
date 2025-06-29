using RS.Commons;
using RS.Widgets.Commons;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSModal : ContentControl, IModal
    {
        static RSModal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSModal), new FrameworkPropertyMetadata(typeof(RSModal)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public void ShowModal(object content)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Content = content;
                this.Visibility = Visibility.Visible;
            });
        }


        public void CloseModal()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Content = null;
                this.Visibility = Visibility.Collapsed;
            });
        }
    }
}
