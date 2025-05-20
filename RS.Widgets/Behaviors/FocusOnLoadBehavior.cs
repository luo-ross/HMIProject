using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Behaviors
{
    public class FocusOnLoadBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnTextBoxLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnTextBoxLoaded;
        }

        private void OnTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Focus();
            if (!string.IsNullOrEmpty(AssociatedObject.Text))
            {
                AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
            }
        }
    }
}
