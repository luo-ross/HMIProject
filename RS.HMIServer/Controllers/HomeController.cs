﻿using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.HMIServer.Areas;
using RS.HMIServer.Models;
using System.Diagnostics;

namespace RS.HMIServer.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : BaseController
    {
        private readonly ILogBLL LogBLL;

        public HomeController(ILogBLL logBLL)
        {
            LogBLL = logBLL;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}