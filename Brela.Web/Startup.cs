using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
//using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser,ApplicationRole>(options =>
                    { 
                        //options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = true;
                //options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                    }
                    )
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
            services.AddElmah<SqlErrorLog>(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("Elmah"); 
            });


            //services.Configure<EmailConfiguration>(Configuration.GetSection("EmailConfiguration"));
            //services.AddLogging();
            //services.AddScoped<UserManager<ApplicationUser>>();

            var columnOptions = new ColumnOptions
            {
                AdditionalDataColumns = new Collection<DataColumn>
                {
                    new DataColumn {DataType = typeof (string), ColumnName = "User"},
                    new DataColumn {DataType = typeof (string), ColumnName = "Other"},
                }
            };
            columnOptions.Store.Add(StandardColumn.LogEvent);
            services.AddSingleton<Serilog.ILogger>
            (x => new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Error)
                .WriteTo.MSSqlServer(Configuration["Serilog:ConnectionString"]
                    ,Configuration["Serilog:TableName"], 
                    columnOptions: columnOptions)
                .CreateLogger());

            var emailConfig = Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();
            //services.AddHealthChecks();
            services.AddHealthCheckService(Configuration);
            services.AddHealthChecksUI();
            services.AddSingleton(emailConfig);
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddTransient<IdentityManager>();
            services.AddTransient<IEmailSender, EmailSender>();
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
