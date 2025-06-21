using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interfaces
{
    public interface IDialog 
    {
        /// <summary>
        /// 获取控件的父级窗体
        /// </summary>
        /// <returns></returns>
        IWindow GetParentWin();

        /// <summary>
        /// 获取Mask类型的加载
        /// </summary>
        /// <returns></returns>
        ILoading GetLoading();

        /// <summary>
        /// 获取Mask类型的对话框
        /// </summary>
        /// <returns></returns>
        IModal GetModal();

        /// <summary>
        /// 获取Mask类型的对话框
        /// </summary>
        /// <returns></returns>
        IMessage GetMessageBox();

        /// <summary>
        /// 获取窗体类型的Modal
        /// </summary>
        /// <returns></returns>
        IModal GetWinModal();

        /// <summary>
        /// 获取窗体类型的消息对话框
        /// </summary>
        /// <returns></returns>
        IMessage GetWinMessageBox();
    }
}
