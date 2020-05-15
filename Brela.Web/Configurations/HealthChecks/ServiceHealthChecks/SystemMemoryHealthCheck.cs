using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sys.Util;

namespace Sys.Web.Configurations.HealthChecks.ServiceHealthChecks
{
    public class SystemMemoryHealthCheck:IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var client = new SystemMemoryMetrics();
            var metrics = client.GetMetrics();
            var percentUsed = 100 * metrics.Used / metrics.Total;
            var status = HealthStatus.Healthy;

            var data = new Dictionary<string, object>();
            data.Add("Total", metrics.Total);
            data.Add("Used", metrics.Used);
            data.Add("Free", metrics.Free);

            HealthCheckResult  result;
            if (percentUsed > 80)
            {
                status = HealthStatus.Healthy;
                result = new HealthCheckResult(status, "Memory is in Optimum", null, data);
            }

            else if (percentUsed > 90)
            {
                status = HealthStatus.Unhealthy;
                result = new HealthCheckResult(status, "System is runing out of memory", null, data);
            }
            else 
                result = new HealthCheckResult(status, "System Memory is degraded", null, data);
            return await Task.FromResult(result);
        }
    }
}
