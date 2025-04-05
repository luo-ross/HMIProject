using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.Models;
using RS.RESTfulApi;
using RS.Annotation.IBLL;
using System.Collections;


namespace RS.Annotation.BLL
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
