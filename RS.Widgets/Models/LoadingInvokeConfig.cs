using NPOI.SS.Formula.Functions;
using RS.Commons;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{

    public class LoadingInvokeConfig
    {
        /// <summary>
        /// 加载类型
        /// </summary>
        public LoadingType LoadingType { get; set; } = LoadingType.ProgressBar;

        /// <summary>
        /// MVVM实体类
        /// </summary>
        public ModelBase ModelBase { get; set; } = null;

        /// <summary>
        /// 是否验证加载
        /// </summary>
        public bool IsLoadingValid { get; set; } = false;

        /// <summary>
        /// 是否显示对话框
        /// </summary>
        public bool IsShowDialog { get; set; } = true;

        /// <summary>
        /// 消息提示框
        /// </summary>
        public RSMessage RSMessage { get; set; }
    }


}
