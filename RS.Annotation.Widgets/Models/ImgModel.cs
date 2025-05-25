using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 图像资源
    /// </summary>
    public partial class ImgModel : ViewModelBase
    {
        public ImgModel(long id, long projectId)
        {
            this.Id = id;
            this.ProjectId = projectId;
        }

        /// <summary>
        /// 图像资源主键 编号
        /// </summary>
        [ObservableProperty]
        private long id;

        /// <summary>
        /// 所属项目
        /// </summary>
        [ObservableProperty]
        private long projectId;


        /// <summary>
        /// 图像名称
        /// </summary>
        [ObservableProperty]
        private string imgName;


        /// <summary>
        /// 图像路径
        /// </summary>
        [ObservableProperty]
        private string imgPath;
     
       

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
                this.SetProperty(ref thubnailImg, value);
            }
        }



        private bool? isCanRead;
        /// <summary>
        /// 图像是否可读取
        /// </summary>
        public bool? IsCanRead
        {
            get
            {
                return isCanRead;
            }
            set
            {
                this.SetProperty(ref isCanRead, value);
            }
        }


        private double width;
        /// <summary>
        /// 图像宽度
        /// </summary>
        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                this.SetProperty(ref width, value);
            }
        }

        private double height;
        /// <summary>
        /// 图像高度
        /// </summary>
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                this.SetProperty(ref height, value);
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
                this.SetProperty(ref isSelect, value);
            }
        }

        private bool isWorking;
        /// <summary>
        /// 是否正在编辑 工作
        /// </summary>
        public bool IsWorking
        {
            get
            {
                return isWorking;
            }
            set
            {
                this.SetProperty(ref isWorking, value);
            }
        }

        private ObservableCollection<RectModel> rectModelList;
        /// <summary>
        /// 标注矩形数据
        /// </summary>
        public ObservableCollection<RectModel> RectModelList
        {
            get
            {
                if (rectModelList == null)
                {
                    rectModelList = new ObservableCollection<RectModel>();
                }
                return rectModelList;
            }
            set
            {
                this.SetProperty(ref rectModelList, value);
            }
        }


        private ObservableCollection<TagSumModel> tagSumModelList;
        /// <summary>
        /// 标注矩形统计
        /// </summary>
        public ObservableCollection<TagSumModel> TagSumModelList
        {
            get
            {
                if (tagSumModelList == null)
                {
                    tagSumModelList = new ObservableCollection<TagSumModel>();
                }
                return tagSumModelList;
            }
            set
            {
                this.SetProperty(ref tagSumModelList, value);
            }
        }

        /// <summary>
        /// 图像缩略图显示占位符 需要提前进行初始化 解决多图像渲染界面卡顿的问题
        /// </summary>
        public Border BorderHost { get; set; }

        /// <summary>
        /// 是否已保存
        /// </summary>
        public bool IsSaved { get; set; }

    }
}
