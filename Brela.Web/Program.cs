using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brela.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sys.Web.Data;
using Sys.Web.Services;

namespace Brela.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host=CreateHostBuilder(args).Build();
            using (var scope=host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var identityManager = services.GetRequiredService<IdentityManager>();
                    context.Database.Migrate();
                    await Seed.SeedData(context,identityManager);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
