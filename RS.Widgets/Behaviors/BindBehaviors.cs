using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace RS.Widgets.Behaviors
{
    public class BindBehaviors
    {

        #region 记录绑定Behaviors
        public static readonly DependencyProperty BehaviorsProperty =
DependencyProperty.RegisterAttached("Behaviors", typeof(BehaviorCollection), typeof(BindBehaviors), new FrameworkPropertyMetadata(null, OnBehaviorsPropertyChanged));

        private static void OnBehaviorsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement frameworkElement)
            {
                return;
            }

            var newBehaviors = e.NewValue as BehaviorCollection;
            var oldBehaviors = e.OldValue as BehaviorCollection;

            if (newBehaviors == oldBehaviors)
            {
                return;
            }
            var behaviors = Interaction.GetBehaviors(frameworkElement);
            frameworkElement.Unloaded -= FrameworkElement_Unloaded;
            if (oldBehaviors != null)
            {
                foreach (var behavior in oldBehaviors)
                {
                    var index = GetIndexOf(behaviors, behavior);
                    if (index >= 0)
                    {
                        behaviors.RemoveAt(index);
                    }
                }
            }

            if (newBehaviors != null)
            {
                foreach (var behavior in newBehaviors)
                {
                    var index = GetIndexOf(behaviors, behavior);
                    if (index < 0)
                    {
                        var clone = (Behavior)behavior.Clone();
                        SetOriginalBehavior(clone, behavior);
                        behaviors.Add(clone);
                    }
                }
            }

            if (behaviors.Count > 0)
            {
                frameworkElement.Unloaded += FrameworkElement_Unloaded;
            }

        }

        private static void FrameworkElement_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement frameworkElement)
            {
                return;
            }


            var behaviors = Interaction.GetBehaviors(frameworkElement);

            foreach (var behavior in behaviors)
            {
                behavior.Detach();
            }

            frameworkElement.Loaded += FrameworkElement_Loaded;
        }

        private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement frameworkElement)
            {
                return;
            }
            frameworkElement.Loaded -= FrameworkElement_Loaded;
            var behaviors = Interaction.GetBehaviors(frameworkElement);
            foreach (var behavior in behaviors)
            {
                behavior.Attach(frameworkElement);
            }
        }

        private static int GetIndexOf(Microsoft.Xaml.Behaviors.BehaviorCollection behaviors, Behavior behavior)
        {
            int index = -1;
            var originalBehavior = GetOriginalBehavior(behavior);

            for (int i = 0; i < behaviors.Count; i++)
            {
                var currentBehavior = behaviors[i];
                if (currentBehavior == behavior || currentBehavior == originalBehavior)
                {
                    index = i;
                    break;
                }

                var currentOriginalBehavior = GetOriginalBehavior(currentBehavior);

                if (currentOriginalBehavior == behavior || currentOriginalBehavior == originalBehavior)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public static BehaviorCollection? GetBehaviors(DependencyObject obj)
        {
            return (BehaviorCollection?)obj.GetValue(BehaviorsProperty);
        }
        public static void SetBehaviors(DependencyObject obj, BehaviorCollection? value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }
        #endregion

        #region 记录原始Behavior
        public static readonly DependencyProperty OriginalBehaviorProperty =
DependencyProperty.RegisterAttached("OriginalBehavior", typeof(Behavior), typeof(BindBehaviors), new UIPropertyMetadata(null));

        public static Behavior? GetOriginalBehavior(DependencyObject obj)
        {
            return (Behavior?)obj.GetValue(OriginalBehaviorProperty);
        }
        public static void SetOriginalBehavior(DependencyObject obj, Behavior? value)
        {
            obj.SetValue(OriginalBehaviorProperty, value);
        }
        #endregion

    }
}
