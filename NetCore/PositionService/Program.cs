using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace MicroService
{
    
    ///<summary>
    ///
    ///</summary>
    public class Program
    {
        ///<summary>
        ///
        ///</summary>
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                //.AddCommandLine(args)
                .AddEnvironmentVariables()//(prefix: "ASPNETCORE_")
                .Build();

            string url = NewMethod(config);

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(url)
                .Build();

            host.Run();
        }

        private static string NewMethod(IConfigurationRoot config)
        {
            Console.WriteLine("URL=" + config["ASPNETCORE_URLS"]);
            return config["ASPNETCORE_URLS"] ?? "http://*:5000";
        }
    }
}
