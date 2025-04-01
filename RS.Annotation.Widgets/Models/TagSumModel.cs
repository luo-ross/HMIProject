using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 标注矩形统计类
    /// </summary>
    public class TagSumModel : ModelBase
    {
        private TagModel tagModel;
        /// <summary>
        /// 标签类别
        /// </summary>
        public TagModel TagModel
        {
            get
            {
                return tagModel;
            }
            set
            {
                OnPropertyChanged(ref tagModel, value);
            }
        }

        private int count;
        /// <summary>
        /// 类别统计
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                OnPropertyChanged(ref count, value);
            }
        }

    }
}
