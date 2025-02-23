using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 标签类别
    /// </summary>
    public class TagModel : ModelBase
    {
        public TagModel(long id, long projectId)
        {
            this.Id = id;
            this.ProjectId = projectId;
        }


        private long id;
        /// <summary>
        /// 标签主键 编号
        /// </summary>
        public long Id
        {
            get
            {
                return id;
            }
            private set
            {
                OnPropertyChanged(ref id, value);
            }
        }

        private long projectId;
        /// <summary>
        /// 所属项目
        /// </summary>
        public long ProjectId
        {
            get
            {
                return projectId;
            }
            private set
            {
                OnPropertyChanged(ref projectId, value);
            }
        }




        private string className;
        /// <summary>
        /// 标签类别名称
        /// </summary>
        [Required(ErrorMessage = "标签类别名称不能为空")]
        public string ClassName
        {
            get
            {
                return className;
            }
            set
            {
                OnPropertyChanged(ref className, value);
            }
        }


        private string tagColor = "#FF0000";
        /// <summary>
        /// 标签类别颜色
        /// </summary>
        [Required(ErrorMessage = "标签类别颜色不能为空")]
        public string TagColor
        {
            get
            {
                return tagColor;
            }
            set
            {
                OnPropertyChanged(ref tagColor, value);
            }
        }


        private string shortCut;
        /// <summary>
        /// 标签快捷键
        /// </summary>
        public string ShortCut
        {
            get
            {
                return shortCut;
            }
            set
            {
                OnPropertyChanged(ref shortCut, value);
            }
        }


        private bool isShortCutAuto;
        /// <summary>
        /// 标签快捷键是否自动生成
        /// </summary>
        public bool IsShortCutAuto
        {
            get
            {
                return isShortCutAuto;
            }
            set
            {
                OnPropertyChanged(ref isShortCutAuto, value);
            }
        }


        private bool isSelect;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelect
        {
            get
            {
                return isSelect;
            }
            set
            {
                OnPropertyChanged(ref isSelect, value);
            }
        }

        /// <summary>
        /// 是否已保存
        /// </summary>
        public bool IsSaved { get; set; }

        public void CloneTo(TagModel tagModel)
        {
            tagModel.Id = this.Id;
            tagModel.TagColor = this.TagColor;
            tagModel.ClassName = this.ClassName;
            tagModel.ShortCut = this.ShortCut;
            tagModel.IsShortCutAuto = this.isShortCutAuto;
            tagModel.IsSaved = this.IsSaved;
            tagModel.IsSelect = this.IsSelect;
        }

    }
}
