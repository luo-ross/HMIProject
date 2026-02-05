using Microsoft.IdentityModel.Tokens;
using RS.Widgets.Interfaces;
using RS.Win32API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    public static class NavigateHelper
    {

        private static readonly Dictionary<object, INavigate> NavigateList = new();

        public static void RegisterNavigate(object dataContext, INavigate navigate)
        {
            if (dataContext != null)
            {
                NavigateList[dataContext] = navigate;
            }
        }

        public static void UnregisterNavigate(object dataContext)
        {
            if (dataContext != null && NavigateList.ContainsKey(dataContext))
            {
                NavigateList.Remove(dataContext);
            }
        }

        public static INavigate? GetNavigate(object dataContext)
        {
            if (dataContext == null)
            {
                return default;
            }
            NavigateList.TryGetValue(dataContext, out var navigate);
            return navigate;
        }


        public static readonly DependencyProperty IsNeedIndentationProperty =
         DependencyProperty.RegisterAttached(
             "IsNeedIndentation",
             typeof(bool),
             typeof(NavigateHelper),
             new PropertyMetadata(false));

        public static bool GetIsNeedIndentation(UIElement element)
        {
            return (bool)element.GetValue(IsNeedIndentationProperty);
        }

        public static void SetIsNeedIndentation(UIElement element, bool value)
        {
            element.SetValue(IsNeedIndentationProperty, value);
        }

    }
}
