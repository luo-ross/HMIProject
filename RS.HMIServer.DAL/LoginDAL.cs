using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.IDAL;

namespace RS.HMIServer.DAL
{

    /// <summary>
    /// 用户数据逻辑层
    /// </summary>
    [ServiceInjectConfig(typeof(ILoginDAL), ServiceLifetime.Transient)]
    internal class LoginDAL : Repository, ILoginDAL
    {
        public LoginDAL(RSAppDbContext rsAppDb)
        {
            this.RSAppDb = rsAppDb;
        }
    }
}
