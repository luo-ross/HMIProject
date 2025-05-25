using RS.Widgets.Models;
using RS.Annotation.Views.Areas.Pictures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Annotation.Views.Areas
{
    public class PicturesViewModel : ViewModelBase
    {


        private double imgWidth = 200;
        /// <summary>
        /// 图像宽度
        /// </summary>
        public double ImgWidth
        {
            get
            {
                return imgWidth;
            }
            set
            {
                this.SetProperty(ref imgWidth, value);
            }
        }


        private double imgHeight = 160;
        /// <summary>
        /// 图像
        /// </summary>
        public double ImgHeight
        {
            get
            {
                return imgHeight;
            }
            set
            {
                this.SetProperty(ref imgHeight, value);
            }
        }


        private int zoomLevel;
        /// <summary>
        /// 图像缩放级别
        /// </summary>
        public int ZoomLevel
        {
            get
            {
                return zoomLevel;
            }
            set
            {
                this.SetProperty(ref zoomLevel, value);
            }
        }


        private ImgModel imgModelSelect;
        /// <summary>
        /// 用户选择图像
        /// </summary>
        public ImgModel ImgModelSelect
        {
            get
            {
                return imgModelSelect;
            }
            set
            {
                this.SetProperty(ref imgModelSelect, value);
            }
        }

        private ObservableCollection<ImgModel> imgModelSelectList;
        /// <summary>
        /// 记录用户的图像选择结果
        /// </summary>
        public ObservableCollection<ImgModel> ImgModelSelectList
        {
            get
            {
                if (imgModelSelectList == null)
                {
                    imgModelSelectList = new ObservableCollection<ImgModel>();
                }
                return imgModelSelectList;
            }
            set
            {
                this.SetProperty(ref imgModelSelectList, value);
            }
        }


        private string log;
        public string Log
        {
            get
            {
                return log;
            }
            set
            {
                this.SetProperty(ref log, value);
            }
        }

        #region 图像亮度和对比度调整
        private double brightness;
        /// <summary>
        /// 图像亮度
        /// </summary>
        public double Brightness
        {
            get
            {
                return brightness;
            }
            set
            {
                this.SetProperty(ref brightness, value);
            }
        }





        private double contrast;
        /// <summary>
        /// 对比度
        /// </summary>
        public double Contrast
        {
            get
            {
                return contrast;
            }
            set
            {
                this.SetProperty(ref contrast, value);
            }
        }

       

        #endregion
    }
}
