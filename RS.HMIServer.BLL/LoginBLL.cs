using Microsoft.Extensions.DependencyInjection;
using RS.HMIServer.Entity;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Models;
using TencentCloud.Ciam.V20220331.Models;
using System.Text;
using Microsoft.Extensions.Primitives;
using RS.HMIServer.Models;

namespace RS.HMIServer.BLL
{
    [ServiceInjectConfig(typeof(ILoginBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class LoginBLL : ILoginBLL
    {
        private readonly ILoginDAL LoginDAL;
        /// <summary>
        /// 加解密服务接口
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;
        public LoginBLL(ILoginDAL loginDAL, ICryptographyBLL cryptographyBLL)
        {
            this.CryptographyBLL = cryptographyBLL;
            this.LoginDAL = loginDAL;
        }
    }
}
