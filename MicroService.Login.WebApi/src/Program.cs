using System.IO;
using System.Net;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MicroService.Login.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var portString = config.GetSection("port").Value;
            var ipString = config.GetSection("ip").Value;

            var port = string.IsNullOrEmpty(portString) ? 5000 : int.Parse(portString);
            var ip = string.IsNullOrEmpty(ipString) ? IPAddress.Any : IPAddress.Parse(ipString);


            BuildWebHost(args, config, ip, port).Run();
        }

        public static IWebHost BuildWebHost(string[] args, IConfigurationRoot config, IPAddress ip, int port) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.AddServerHeader = false;
                    options.Listen(ip, port);
                })
                .ConfigureServices(services => services.AddAutofac())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();
    }
}