using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace dotnetcore_frontdoorservice_issue
{
    public class Startup
    {
        private readonly ILogger _logger;
        
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = 
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                options.ForwardLimit = 2;
                
                // Put your front door FQDN here
                options.AllowedHosts = new List<string>() { "fdissue.azurefd.net" };
            });

            services.AddHttpContextAccessor();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders();
            
            app.Use(async (context, next) =>
            {
                // Request method, scheme, and path
                _logger.LogInformation("Request Method: {METHOD}", context.Request.Method);
                _logger.LogInformation("Request Scheme: {SCHEME}", context.Request.Scheme);
                _logger.LogInformation("Request Path: {PATH}", context.Request.Path);

                // Headers
                foreach (var header in context.Request.Headers)
                {
                    _logger.LogInformation("Header: {KEY}: {VALUE}", header.Key, header.Value);
                }

                // Connection: RemoteIp
                _logger.LogInformation("Request RemoteIp: {REMOTE_IP_ADDRESS}", 
                    context.Connection.RemoteIpAddress);

                await next();
            });

            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
