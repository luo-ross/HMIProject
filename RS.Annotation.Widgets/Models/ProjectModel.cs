using RS.Commons.Enums;
using RS.Models;
using RS.Widgets.Models;
using RS.Annotation.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RS.Widgets.Models
{
    public class ProjectModel : ModelBase
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="id">必须初始化id值</param>
        public ProjectModel(long id)
        {
            this.Id = id;
        }


        private long id;
        /// <summary>
        /// 项目主键
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


        private TaskEnum tasks;
        /// <summary>
        /// 深度学习任务类型
        /// </summary>
        public TaskEnum Tasks
        {
            get
            {
                return tasks;
            }
            set
            {
                OnPropertyChanged(ref tasks, value);
            }
        }



        private string projectName;

        [Required(ErrorMessage = "项目名称不能为空")]
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName
        {
            get
            {
                return projectName;
            }
            set
            {
                OnPropertyChanged(ref projectName, value);
            }
        }


        private string projectPath;
        [Required(ErrorMessage = "项目文件路径不能为空")]
        /// <summary>
        /// 项目文件路径
        /// </summary>
        public string ProjectPath
        {
            get
            {
                return projectPath;
            }
            set
            {
                OnPropertyChanged(ref projectPath, value);
            }
        }


        private string description;
        /// <summary>
        /// 项目说明
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                OnPropertyChanged(ref description, value);
            }
        }


        private BitmapSource thubnailImg;
        /// <summary>
        /// 缩略图
        /// </summary>
        public BitmapSource ThubnailImg
        {
            get
            {
                return thubnailImg;
            }
            set
            {
                OnPropertyChanged(ref thubnailImg, value);
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


        private bool isInit;
        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool IsInit
        {
            get
            {
                return isInit;
            }
            set
            {
                OnPropertyChanged(ref isInit, value);
            }
        }


        private ObservableCollection<TagModel> tagModelList;
        /// <summary>
        /// 标签类别
        /// </summary>
        public ObservableCollection<TagModel> TagModelList
        {
            get
            {
                if (tagModelList == null)
                {
                    tagModelList = new ObservableCollection<TagModel>();
                    //for (int i = 0; i < 10; i++)
                    //{
                    //   GenerateRandomColor();
                    //    tagModelList.Add(new TagModel()
                    //    {
                    //        IsShortCutAuto = false,
                    //        ClassName = $"类别{i+1}",
                    //        Id = i,
                    //        IsSelect = false,
                    //        ShortCut = "1",
                    //        TagColor = GenerateRandomColor(),
                    //        IsSaved = false,
                    //        IsLoading = false
                    //    });
                    //}

                }
                return tagModelList;
            }
            set
            {
                OnPropertyChanged(ref tagModelList, value);
            }
        }




        private ObservableCollection<ImgModel> imgModelList;
        /// <summary>
        /// 图像资源
        /// </summary>
        public ObservableCollection<ImgModel> ImgModelList
        {
            get
            {
                if (imgModelList == null)
                {
                    imgModelList = new ObservableCollection<ImgModel>();
                }
                return imgModelList;
            }
            set
            {
                OnPropertyChanged(ref imgModelList, value);
            }
        }


        private DateTime createTime;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                return createTime;
            }
            set
            {
                OnPropertyChanged(ref createTime, value);
            }
        }


        private DateTime updateTime;
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime
        {
            get
            {
                return updateTime;
            }
            set
            {
                OnPropertyChanged(ref updateTime, value);
            }
        }

        /// <summary>
        /// 是否已保存
        /// </summary>
        public bool IsSaved { get; set; }
    }
}
