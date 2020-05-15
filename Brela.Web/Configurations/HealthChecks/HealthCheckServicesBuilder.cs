using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.Network.Core;
using HealthChecks.System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sys.Web.Configurations.HealthChecks.ServiceHealthChecks;

namespace Sys.Web.Configurations.HealthChecks
{
    public static class HealthCheckServicesBuilder
    {
        public static IHealthChecksBuilder AddHealthCheckService(this IServiceCollection services,IConfiguration configuration)
        {
            var builder = services.AddHealthChecks();
            builder.AddDiskStorageHealthCheck(delegate(DiskStorageOptions diskStorageOptions)
                {
                    diskStorageOptions.AddDrive(@"c:\",5000);
                },"System Storage",HealthStatus.Degraded);

            //builder.AddUrlGroup(new Uri("https://www.youtube.com/"),
            //    name: "Base URL",
            //    failureStatus: HealthStatus.Degraded);
            //builder.AddSmtpHealthCheck(x => { x.Host = "mailserver"; x.Port = 110; x.ConnectionType = SmtpConnectionType.TLS; });
            builder.AddCheck<SystemMemoryHealthCheck>("Memory");
            builder.AddCheck("System Database", new HealthCheckMainDbContext(configuration.GetConnectionString("DefaultConnection")));
            return builder;
        }
    }
}
