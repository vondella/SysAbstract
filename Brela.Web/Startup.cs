using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Brela.Web.Data;
using Brela.Web.Middlewares;
using Brela.Web.Models;
using ElmahCore.Mvc;
using ElmahCore.Sql;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Sys.Jobs;
using Sys.Web.Configurations;
using Sys.Web.Configurations.HealthChecks;
using Sys.Web.Services;

namespace Brela.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper(typeof(Startup));
            ServiceConfigurations.ConfigureIdentity(services, Configuration);
            ServiceConfigurations.ConfigureElmah(services, Configuration);
            ServiceConfigurations.ConfigureSerilog(services, Configuration);
            var emailConfig = Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();
            //health checks
            services.AddHealthCheckService(Configuration);
            services.AddHealthChecksUI();

            services.AddSingleton(emailConfig);
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddTransient<IdentityManager>();
            services.AddTransient<IEmailSender, EmailSender>();

            // job schedule registrations 
            services.AddSingleton<BackgroundWorker>();  
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseMiddleware<ErrorLoggingMiddleware>();

            //app.UseHttpsRedirection();

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/elmah", StringComparison.OrdinalIgnoreCase), appBuilder =>
            {
                appBuilder.Use(next =>
                {
                    return async ctx =>
                    {
                        ctx.Features.Get<IHttpBodyControlFeature>().AllowSynchronousIO = true;

                        await next(ctx);
                    };
                });
            });
            app.UseElmah();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.UseHealthChecksUI();
            //    .UseHealthChecksUI(setup =>
            //{
            //    setup.AddCustomStylesheet(@"wwwroot\css\dotnet.css");
            //});
        }
    }
}
