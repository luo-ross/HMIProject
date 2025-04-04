using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.IBLL;
using RS.Models;
using RS.RESTfulApi;
using System.Collections;


namespace RS.HMI.BLL
{
    [ServiceInjectConfig(typeof(IGeneralBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class GeneralBLL : IGeneralBLL
    {
        private readonly IMemoryCache MemoryCache;
        private readonly ICryptographyBLL CryptographyBLL;
        public GeneralBLL(ICryptographyBLL cryptographyBLL, IMemoryCache memoryCache)
        {
            CryptographyBLL = cryptographyBLL;
            MemoryCache = memoryCache;
        }
    }
}
