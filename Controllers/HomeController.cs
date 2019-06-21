using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dotnetcore_frontdoorservice_issue.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets.Internal;

namespace dotnetcore_frontdoorservice_issue.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public IActionResult Index()
        {
            // Docs here: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-2.2
            // Say that the X-Forwarded-Host header value should override this if using the Forwarded Headers Middleware
            ViewData["HostName"] = _httpContextAccessor.HttpContext.Request.Host;
            ViewData["Method"] = _httpContextAccessor.HttpContext.Request.Method;
            ViewData["Scheme"] = _httpContextAccessor.HttpContext.Request.Scheme;
            ViewData["Path"] = _httpContextAccessor.HttpContext.Request.Path;
            
            var headers = new Dictionary<string, string>();
            foreach (var (key, value) in _httpContextAccessor.HttpContext.Request.Headers)
            {
                headers.Add(key, value);
            }

            ViewData["Headers"] = headers;
            ViewData["RemoteIp"] = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
           
            return View();
        }

        public IActionResult Privacy()
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
