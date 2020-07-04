using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace SiteSubscriptionServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(loggerFactory => loggerFactory.AddEventLog())
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                              //.UseKestrel(options =>
                              //{
                              //    options.Listen(IPAddress.Any, 443, listenOptions =>
                              //    {
                              //        listenOptions.UseHttps(@"I:\SignalRTest\star_gensolve_com.pfx", "Rx95b0lz");
                              //    });
                              //});
                });
    }
}
