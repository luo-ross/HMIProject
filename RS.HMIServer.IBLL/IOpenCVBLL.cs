using RS.Commons;
using RS.HMIServer.Models;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.IBLL
{
    public interface IOpenCVBLL
    {
        Task<OperateResult<ImgVerifyInitModel>> GetVerifyImgInitModelAsync();
    }
}
