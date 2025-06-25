using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RS.HMI.Client.Models
{
    /// <summary>
    /// 导航菜单
    /// </summary>
    public class NavigateModel : ObservableObject
    {
        private IconKey iconKey;

        public IconKey IconKey
        {
            get { return iconKey; }
            set
            {
                this.SetProperty(ref iconKey, value);
            }
        }



        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                this.SetProperty(ref name, value);
            }
        }


        private bool hasChildren;

        public bool HasChildren
        {
            get { return hasChildren; }
            set
            {
                this.SetProperty(ref hasChildren, value);
            }
        }


        private ObservableObject viewMoel;
        /// <summary>
        /// 绑定的ViewModel 有了ViewModel 就有视图了
        /// </summary>
        public ObservableObject ViewMoel
        {
            get { return viewMoel; }
            set
            {
                this.SetProperty(ref viewMoel, value);
            }
        }


        private ObservableCollection<NavigateModel> childList;

        public ObservableCollection<NavigateModel> ChildList
        {
            get { return childList; }
            set
            {
                this.SetProperty(ref childList, value);
            }
        }


    }
}
