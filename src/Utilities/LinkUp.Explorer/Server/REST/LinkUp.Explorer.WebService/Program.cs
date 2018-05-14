using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LinkUp.Explorer.WebService
{
    public class Program
    {
        public static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("hosting.json", optional: true)
         .Build();
            var host = new WebHostBuilder()
        .UseKestrel()
        .UseConfiguration(config)
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>()
        .Build();
            return host;
        }

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
    }
}