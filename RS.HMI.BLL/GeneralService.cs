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
    [ServiceInjectConfig(typeof(IGeneralService), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class GeneralService : IGeneralService
    {
        private readonly IMemoryCache MemoryCache;
        private readonly ICryptographyService CryptographyService;
        public GeneralService(ICryptographyService cryptographyService, IMemoryCache memoryCache)
        {
            CryptographyService = cryptographyService;
            MemoryCache = memoryCache;
        }
    }
}
