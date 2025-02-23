using RS.Widgets.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RS.WPFApp.Views.Areas
{
    public class AnnotationViewModel : ModelBase
    {

        public AnnotationViewModel()
        {
            this.UpdateOpacityDes();
            this.UpdateScaleDes();
        }
        private BitmapSource imgSource;
        /// <summary>
        /// 图像资源
        /// </summary>
        public BitmapSource ImgSource
        {
            get
            {
                return imgSource;
            }
            set
            {
                OnPropertyChanged(ref imgSource, value);
            }
        }

        private Point annotationCanvasHostMouseMovePosition;
        /// <summary>
        /// 标注视窗鼠标移动位置
        /// </summary>
        public Point AnnotationCanvasHostMouseMovePosition
        {
            get
            {
                return annotationCanvasHostMouseMovePosition;
            }
            set
            {
                OnPropertyChanged(ref annotationCanvasHostMouseMovePosition, value);
            }
        }


        private Point annotationCanvasMouseMovePosition;
        /// <summary>
        /// 标注Canvas鼠标移动位置
        /// </summary>
        public Point AnnotationCanvasMouseMovePosition
        {
            get
            {
                return annotationCanvasMouseMovePosition;
            }
            set
            {
                OnPropertyChanged(ref annotationCanvasMouseMovePosition, value);
            }
        }


        private Point canvasMouseDownTranslateTransformPosition;
        /// <summary>
        /// 记录当前鼠标按下时的平移变换位置
        /// </summary>
        public Point CanvasMouseDownTranslateTransformPosition
        {
            get
            {
                return canvasMouseDownTranslateTransformPosition;
            }
            set
            {
                OnPropertyChanged(ref canvasMouseDownTranslateTransformPosition, value);
            }
        }



        #region 图像缩放属性



        private double centerX;
        /// <summary>
        /// 缩放中心X
        /// </summary>
        public double CenterX
        {
            get
            {
                return centerX;
            }
            set
            {
                OnPropertyChanged(ref centerX, value);
            }
        }

        private double centerY;
        /// <summary>
        /// 缩放中心Y
        /// </summary>
        public double CenterY
        {
            get
            {
                return centerY;
            }
            set
            {
                OnPropertyChanged(ref centerY, value);
            }
        }

        private double scale = 1;
        /// <summary>
        /// 缩放
        /// </summary>
        public double Scale
        {
            get
            {
                return scale;
            }
            set
            {
                OnPropertyChanged(ref scale, value);
                this.UpdateScaleDes();
            }
        }

        public void UpdateScaleDes()
        {
            if (this.Scale < 1)
            {
                this.ScaleDes = $"1:{Math.Floor(1 / this.Scale)}";
            }
            else
            {
                this.ScaleDes = $"{Math.Floor(this.Scale)}:1";
            }
        }


        private string scaleDes;
        /// <summary>
        /// 缩放描述
        /// </summary>
        public string ScaleDes
        {
            get
            {
                return scaleDes;
            }
            set
            {
                OnPropertyChanged(ref scaleDes, value);
            }
        }


        private double scaleX;
        /// <summary>
        /// X缩放
        /// </summary>
        public double ScaleX
        {
            get
            {
                return scaleX;
            }
            set
            {
                OnPropertyChanged(ref scaleX, value);
            }
        }


        private double scaleY;
        /// <summary>
        /// Y缩放
        /// </summary>
        public double ScaleY
        {
            get
            {
                return scaleY;
            }
            set
            {
                OnPropertyChanged(ref scaleY, value);
            }
        }


        private double minScale = 1;
        /// <summary>
        /// 最小缩放
        /// </summary>
        public double MinScale
        {
            get
            {
                return minScale;
            }
            set
            {
                OnPropertyChanged(ref minScale, value);
            }
        }


        private double maxScale = 96;
        /// <summary>
        /// 最大缩放
        /// </summary>
        public double MaxScale
        {
            get
            {
                return maxScale;
            }
            set
            {
                OnPropertyChanged(ref maxScale, value);
            }
        }
        #endregion

        #region 图像平移属性
        private double transformX;
        /// <summary>
        /// X方向平移值
        /// </summary>
        public double TransformX
        {
            get
            {
                return transformX;
            }
            set
            {
                OnPropertyChanged(ref transformX, value);
            }
        }


        private double transformY;
        /// <summary>
        /// Y方向平移值
        /// </summary>
        public double TransformY
        {
            get
            {
                return transformY;
            }
            set
            {
                OnPropertyChanged(ref transformY, value);
            }
        }
        #endregion

        private TagModel tagModelSelect;
        /// <summary>
        /// 选中的标注标签
        /// </summary>
        public TagModel TagModelSelect
        {
            get
            {
                return tagModelSelect;
            }
            set
            {
                OnPropertyChanged(ref tagModelSelect, value);
            }
        }

        private byte opacity = 1;
        /// <summary>
        /// 矩形框的透明度
        /// </summary>
        public byte Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                OnPropertyChanged(ref opacity, value);
                this.UpdateOpacityDes();
            }
        }

        private string opacityDes;
        /// <summary>
        /// 矩形框的透明度描述
        /// </summary>
        public string OpacityDes
        {
            get
            {
                return opacityDes;
            }
            set
            {
                OnPropertyChanged(ref opacityDes, value);
            }
        }

        public void UpdateOpacityDes()
        {
            this.OpacityDes = $"{(int)(Opacity / 255D * 100)}%";
        }

        #region 矩形选择

        private RectModel rectModelSelect;
        /// <summary>
        /// 当前矩形选择
        /// </summary>
        public RectModel RectModelSelect
        {
            get
            {
                return rectModelSelect;
            }
            set
            {
                OnPropertyChanged(ref rectModelSelect, value);
            }
        }




        #endregion

        #region 导航矩形框
        private RectModel navRectModel;
        /// <summary>
        /// 导航矩形框
        /// </summary>
        public RectModel NavRectModel
        {
            get
            {
                if (navRectModel==null)
                {
                    navRectModel = new RectModel();
                }
                return navRectModel;
            }
            set
            {
                OnPropertyChanged(ref navRectModel, value);
            }
        }
        #endregion

        private CRUD crud;
        /// <summary>
        /// 增删改查
        /// </summary>
        public CRUD CRUD
        {
            get
            {
                return crud;
            }
            set
            {
                OnPropertyChanged(ref crud, value);
            }
        }


        private TagModel tagModelEdit;
        /// <summary>
        /// 标签编辑
        /// </summary>
        public TagModel TagModelEdit
        {
            get
            {
                return tagModelEdit;
            }
            set
            {
                OnPropertyChanged(ref tagModelEdit, value);
            }
        }
    }
}
