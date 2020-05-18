using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brela.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sys.Jobs;
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
            Thread counterBackgroundWorkerThread = new Thread(CounterHandlerAsync)
            {
                IsBackground = true
            };
            counterBackgroundWorkerThread.Start(host.Services);
            host.Run();
        }

        private static void CounterHandlerAsync(object obj)
        {
            // Here we check that the provided parameter is, in fact, an IServiceProvider
            IServiceProvider provider = obj as IServiceProvider
                                        ?? throw new ArgumentException($"Passed in thread parameter was not of type {nameof(IServiceProvider)}", nameof(obj));

            // Using an infinite loop for this demonstration but it all depends on the work you want to do.
            while (true)
            {
                // Here we create a new scope for the IServiceProvider so that we can get already built objects from the Inversion Of Control Container.
                using (IServiceScope scope = provider.CreateScope())
                {
                    // Here we retrieve the singleton instance of the BackgroundWorker.
                    BackgroundWorker backgroundWorker = scope.ServiceProvider.GetRequiredService<BackgroundWorker>();

                    // And we execute it, which will log out a number to the console
                    backgroundWorker.Execute();
                }

                // This is only placed here so that the console doesn't get spammed with too many log lines
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
