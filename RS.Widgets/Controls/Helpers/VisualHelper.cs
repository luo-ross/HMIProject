using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RS.Widgets.Controls
{
    public static class VisualHelper
    {

        public static T? TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            var parentObject = child.GetParentObject();
            while (parentObject is not null)
            {
                if (parentObject is T objectToFind)
                {
                    return objectToFind;
                }

                parentObject = parentObject.GetParentObject();
            }
            return null;
        }

       

        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent is not null)
            {
                yield return parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        public static IEnumerable<DependencyObject> GetVisualAncestry(this DependencyObject? leaf)
        {
            while (leaf is not null)
            {
                yield return leaf;
                leaf = leaf is Visual or Visual3D
                    ? VisualTreeHelper.GetParent(leaf)
                    : LogicalTreeHelper.GetParent(leaf);
            }
        }


        public static T? GetVisualAncestor<T>(this DependencyObject? leaf) where T : DependencyObject
        {
            while (leaf is not null)
            {
                if (leaf is T ancestor)
                {
                    return ancestor;
                }

                leaf = leaf is Visual or Visual3D
                    ? VisualTreeHelper.GetParent(leaf)
                    : LogicalTreeHelper.GetParent(leaf);
            }

            return default;
        }


        public static T? FindChild<T>(this DependencyObject? parent, string? childName = null) where T : DependencyObject
        {
            if (parent is null)
            {
                return null;
            }

            T? foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is not T currentChild)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild is not null)
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    if (currentChild is IFrameworkInputElement frameworkInputElement && frameworkInputElement.Name == childName)
                    {
                        foundChild = currentChild;
                        break;
                    }

                    foundChild = FindChild<T>(currentChild, childName);
                    if (foundChild is not null)
                    {
                        break;
                    }
                }
                else
                {
                    foundChild = currentChild;
                    break;
                }
            }

            return foundChild;
        }


        public static IEnumerable<T> FindChildren<T>(this DependencyObject? source, bool forceUsingTheVisualTreeHelper = false)
            where T : DependencyObject
        {
            if (source is not null)
            {
                var childObjects = source.GetChildObjects(forceUsingTheVisualTreeHelper);
                foreach (var child in childObjects)
                {
                    if (child is T childToFind)
                    {
                        yield return childToFind;
                    }
                    foreach (T descendant in FindChildren<T>(child, forceUsingTheVisualTreeHelper))
                    {
                        yield return descendant;
                    }
                }
            }
        }


        public static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject? parent, bool forceUsingTheVisualTreeHelper = false)
        {
            if (parent is not null)
            {
                if (!forceUsingTheVisualTreeHelper && (parent is ContentElement || parent is FrameworkElement))
                {
                    foreach (var obj in LogicalTreeHelper.GetChildren(parent))
                    {
                        if (obj is DependencyObject dependencyObject)
                        {
                            yield return dependencyObject;
                        }
                    }
                }
                else if (parent is Visual || parent is Visual3D)
                {
                    int count = VisualTreeHelper.GetChildrenCount(parent);
                    for (int i = 0; i < count; i++)
                    {
                        yield return VisualTreeHelper.GetChild(parent, i);
                    }
                }
            }
        }


        public static T? TryFindFromPoint<T>(UIElement reference, Point point) where T : DependencyObject
        {
            if (!(reference.InputHitTest(point) is DependencyObject element))
            {
                return null;
            }

            if (element is T theObject)
            {
                return theObject;
            }

            return element.TryFindParent<T>();
        }

        public static bool IsDescendantOf(this DependencyObject node, DependencyObject reference)
        {
            DependencyObject? currentNode = node;

            while (currentNode is not null)
            {
                if (currentNode == reference)
                {
                    return true;
                }

                if (currentNode is Popup popup)
                {
                    currentNode = popup.Parent ?? popup.PlacementTarget;
                }
                else
                {
                    currentNode = currentNode.GetParentObject();
                }
            }

            return false;
        }



        public static DependencyObject? GetParentObject(this DependencyObject? child)
        {
            if (child is null)
            {
                return null;
            }

            if (child is ContentElement contentElement)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent is not null)
                {
                    return parent;
                }

                return contentElement is FrameworkContentElement fce ? fce.Parent : null;
            }

            var childParent = VisualTreeHelper.GetParent(child);
            if (childParent is not null)
            {
                return childParent;
            }

            if (child is FrameworkElement frameworkElement)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent is not null)
                {
                    return parent;
                }
            }

            return null;
        }
    }
}
