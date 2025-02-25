using RS.Commons;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFApp.IBLL
{
    public interface IGeneralService
    {
        /// <summary>
        /// 获取会话
        /// </summary>
        /// <returns></returns>
        Task<OperateResult> GetSessionModelAsync();
    }
}
