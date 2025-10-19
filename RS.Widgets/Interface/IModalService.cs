using RS.Commons;
using RS.Widgets.Commons;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Interface
{
    internal interface IModalService
    {
        /// <summary>
        /// 显示模态
        /// </summary>
        void ShowModal(object modalContent);

        /// <summary>
        /// 隐藏模态
        /// </summary>
        void HideModal();
    }
}
