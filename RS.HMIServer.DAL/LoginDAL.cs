
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RS.HMIServer.Entity;
using RS.HMIServer.DAL.Redis;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.IDAL;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Models;
using StackExchange.Redis;
using RS.HMIServer.Models;
using RTools_NTS.Util;
using Newtonsoft.Json.Linq;
using RS.Commons.Extensions;

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
