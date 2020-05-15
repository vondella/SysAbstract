using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brela.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sys.Web.Configurations.HealthChecks.ServiceHealthChecks
{
    public class HealthCheckMainDbContext:IHealthCheck
    {
        private readonly ApplicationDbContext _context;
        private static readonly string DefaultQuery = "select 1";
        public  string ConnectionString { get; set; }
        public  string TestQuery { get; set; }
        public HealthCheckMainDbContext(string connectionString):this(connectionString,testQuery:DefaultQuery)
        {
        }

        public HealthCheckMainDbContext(string connectionString,string testQuery)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            TestQuery = testQuery;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var connection=new SqlConnection(ConnectionString))
            {
                try
                {
                //    if (await _context.Database.CanConnectAsync(cancellationToken))
                //        return HealthCheckResult.Healthy("System can connect to main Database");
                //    return HealthCheckResult.Unhealthy("System couldn't connect to database (Database is down)");
                await connection.OpenAsync(cancellationToken);
                if (TestQuery != null)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = TestQuery;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
                }
                catch (Exception ex)
                {
                    return new HealthCheckResult(status:context.Registration.FailureStatus,exception:ex);
                }
            }
           return  HealthCheckResult.Healthy("Database connection is healthy");
        }
    }
}
