using CommunityToolkit.Mvvm.Input;
using RS.Commons;
using RS.Commons.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RS.Widgets.Interface
{
    /// <summary>
    /// 增删改查表单接口
    /// </summary>
    public interface IFormService
    {
        /// <summary>
        /// 表单提交
        /// </summary>
        IAsyncRelayCommand FormSubmitCommand { get; }

        /// <summary>
        /// 表单增删改查类型
        /// </summary>
        CRUD CRUD { get; set; }
    }
}
