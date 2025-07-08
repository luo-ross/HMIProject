using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 导航菜单
    /// </summary>
    public class NavigateModel : ObservableObject
    {
        /// <summary>
        /// 数据主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 父级
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }


        private IconKey iconKey;
        public IconKey IconKey
        {
            get { return iconKey; }
            set
            {
                this.SetProperty(ref iconKey, value);
            }
        }


        private bool isSelect;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelect
        {
            get { return isSelect; }
            set
            {
                this.SetProperty(ref isSelect, value);
            }
        }


        private bool isExpand;
        /// <summary>
        /// 是否展开
        /// </summary>
        public bool IsExpand
        {
            get { return isExpand; }
            set
            {
                this.SetProperty(ref isExpand, value);
            }
        }


        private string navName;
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string NavName
        {
            get { return navName; }
            set
            {
                this.SetProperty(ref navName, value);
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


        private bool isGroupNav;
        /// <summary>
        /// 是否是分组当行
        /// </summary>
        public bool IsGroupNav
        {
            get { return isGroupNav; }
            set
            {
                this.SetProperty(ref isGroupNav, value);
            }
        }


        private string viewKey;
        /// <summary>
        /// 视图主键
        /// </summary>
        public string ViewKey
        {
            get { return viewKey; }
            set
            {
                this.SetProperty(ref viewKey, value);
            }
        }



        private INotifyPropertyChanged viewMoel;
        /// <summary>
        /// 绑定的ViewModel 有了ViewModel 就有视图了
        /// </summary>
        public INotifyPropertyChanged ViewMoel
        {
            get { return viewMoel; }
            set
            {
                this.SetProperty(ref viewMoel, value);
            }
        }
      
    }
}
