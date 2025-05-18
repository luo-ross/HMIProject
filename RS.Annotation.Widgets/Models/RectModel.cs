using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace RS.Widgets.Models
{
    /// <summary>
    /// 标注矩形类
    /// </summary>
    public class RectModel : ModelBase
    {

        public RectModel()
        {
          
        }
        public RectModel(long id, long pictureId, long projectId)
        {
            this.Id = id;
            this.PictureId = pictureId;
            this.ProjectId = projectId;
        }


        private long id;
        /// <summary>
        /// 矩形主键 编号
        /// </summary>
        public long Id
        {
            get
            {
                return id;
            }
            private set
            {
                this.SetProperty(ref id, value);
            }
        }


        private long pictureId;
        /// <summary>
        /// 矩形所属图像 主键
        /// </summary>
        public long PictureId
        {
            get
            {
                return pictureId;
            }
            private set
            {
                this.SetProperty(ref pictureId, value);
            }
        }

        private long projectId;
        /// <summary>
        /// 矩形所属项目 主键
        /// </summary>
        public long ProjectId
        {
            get
            {
                return projectId;
            }
            set
            {
                this.SetProperty(ref projectId, value);
            }
        }


        private TagModel tagModel;
        /// <summary>
        /// 矩形关联标签
        /// </summary>
        public TagModel TagModel
        {
            get
            {
                return tagModel;
            }
            set
            {
                this.SetProperty(ref tagModel, value);
            }
        }


        private double canvasLeft;
        /// <summary>
        /// 矩形左上角X坐标
        /// </summary>
        public double CanvasLeft
        {
            get
            {
                return canvasLeft;
            }
            set
            {
                this.SetProperty(ref canvasLeft, value);
            }
        }


        private double canvasTop;
        /// <summary>
        /// 矩形左上角Y坐标
        /// </summary>
        public double CanvasTop
        {
            get
            {
                return canvasTop;
            }
            set
            {
                this.SetProperty(ref canvasTop, value);
            }
        }



        private double width;
        /// <summary>
        /// 矩形宽度
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
        /// 矩形高度
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


        private double angle;
        /// <summary>
        /// 矩形旋转角度
        /// </summary>
        public double Angle
        {
            get
            {
                return angle;
            }
            set
            {
                this.SetProperty(ref angle, value);
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

        /// <summary>
        /// 是否已保存
        /// </summary>
        public bool IsSaved { get; set; }

        /// <summary>
        /// 记录画在Canvas上的矩形
        /// </summary>
        public Rectangle Rectangle { get; set; }




        //public RectModel Clone()
        //{
        //    RectModel rectModel = new RectModel()
        //    {

        //        Angle=this.Angle,
        //        CanvasLeft=this.CanvasLeft,
        //        CanvasTop=this.CanvasTop,
        //        Height=this.Height,
        //        Id=,
        //        IsLoading=this.IsLoading,
        //        IsSaved=this.IsSaved,
        //        IsSelect=this.IsSelect,
        //        PictureId=this.PictureId,
        //        ProjectId=this.ProjectId,
        //        TagModel=this.TagModel.Clone(),
        //        Width=this.Width,
        //        Rectangle=null,
        //    };
        //    return rectModel;
        //}
    }
}
