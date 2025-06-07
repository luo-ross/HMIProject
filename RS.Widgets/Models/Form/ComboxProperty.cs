using RS.Widgets.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models.Form
{
    public class ComboxProperty : PropertyBase
    {

        private string selectedValuePath;
        /// <summary>
        /// 自定义数据配置选择值的路径
        /// </summary>
        public string SelectedValuePath
        {
            get { return selectedValuePath; }
            set
            {
                this.SetProperty(ref selectedValuePath, value);
            }
        }


        private string displayMemberPath;
        /// <summary>
        /// 自定义数据配置选择显示值的路径
        /// </summary>
        public string DisplayMemberPath
        {
            get { return displayMemberPath; }
            set
            {
                this.SetProperty(ref displayMemberPath, value);
            }
        }


        private string watermark = "请选择内容";
        /// <summary>
        /// 自定义水印
        /// </summary>
        public override string Watermark
        {
            get { return watermark; }
            set
            {
                this.SetProperty(ref watermark, value);
            }
        }

        private ObservableCollection<object> dataSource;
        /// <summary>
        /// 数绑定
        /// </summary>
        public ObservableCollection<object> DataSource
        {
            get { return dataSource; }
            set
            {
                this.SetProperty(ref dataSource, value);
            }
        }
    }
}
