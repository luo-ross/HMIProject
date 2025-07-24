using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RS.Win32API;
using RS.Win32API.Structs;

namespace RS.Widgets.Extensions
{
    public static class HitTestExtension
    {

        public static (int Index, UIElement Child)? GetMouseOverChildInfo(this Panel panel)
        {
            POINT screenPoint = new POINT();
            NativeMethods.GetCursorPos(screenPoint);
            Point mousePosition = panel.PointFromScreen(new Point(screenPoint.x, screenPoint.y));
            var hitTestResult = VisualTreeHelper.HitTest(panel, mousePosition);
            DependencyObject current = hitTestResult?.VisualHit;

            while (current != null && current != panel)
            {
                int index = panel.Children.IndexOf(current as UIElement);
                if (index >= 0)
                {
                    return (index, panel.Children[index]);
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        public static bool IsDescendantOf(this DependencyObject descendant, DependencyObject ancestor)
        {
            var current = descendant;
            while (current != null)
            {
                if (current == ancestor)
                {
                    return true;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }


        public static DependencyObject GetElementAtPointOfType(this UIElement root, Point point, Type type)
        {
            HitTestResult result = VisualTreeHelper.HitTest(root, point);
            if (result == null)
            {
                return null;
            }

            DependencyObject current = result.VisualHit;
            while (current != null)
            {
                if (type.IsInstanceOfType(current))
                {
                    return current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        public static T GetUIElementUnderMouse<T>(this UIElement root, Point mousePosition) where T : UIElement
        {
            var hitTestResult = VisualTreeHelper.HitTest(root, mousePosition);
            var visualHit = hitTestResult?.VisualHit;

            while (visualHit != null)
            {
                var child = visualHit as T;
                if (child != null)
                {
                    return child;
                }
                visualHit = VisualTreeHelper.GetParent(visualHit);
            }
            return null;
        }


        public static List<DependencyObject> GetAllElementsAtPoint(
            this Visual root,
            Point point,
            Predicate<DependencyObject> filter = null)
        {
            var results = new List<DependencyObject>();
            VisualTreeHelper.HitTest(root, filter == null ? null : new HitTestFilterCallback(obj =>
                {
                    if (filter(obj))
                    {
                        return HitTestFilterBehavior.Continue;
                    }
                    else
                    {
                        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                    }

                }),
                new HitTestResultCallback(hit =>
                {
                    if (hit.VisualHit is DependencyObject depObj)
                    {
                        results.Add(depObj);
                    }
                    return HitTestResultBehavior.Continue;
                }),
                new PointHitTestParameters(point)
            );
            return results;
        }
    }
}
