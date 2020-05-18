using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Brela.Web.Data;
using Brela.Web.Models;
using ElmahCore.Mvc;
using ElmahCore.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Sys.Web.Configurations
{
    public class ServiceConfigurations
    {

        public static void ConfigureIdentity(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
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
        }
        public static void ConfigureElmah(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddElmah<SqlErrorLog>(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("Elmah");
            });

        }

        public static void ConfigureSerilog(IServiceCollection services, IConfiguration Configuration)
        {
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
                    , Configuration["Serilog:TableName"],
                    columnOptions: columnOptions)
                .CreateLogger());
        }
    }
}
