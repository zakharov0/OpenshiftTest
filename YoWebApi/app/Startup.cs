using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.EntityFrameworkCore;

namespace VesselService
{
	
	
    public class ResultSettings
    {
        //public AppOptions()
        //{
            // Set default value.
        //    Option1 = "value1_from_ctor";
        //}
        //public string Option1 { get; set; }
        //public int Option2 { get; set; } = 5;
        public string ConnectionString { get; set; }
        public int Limit { get; set; }
    }
    
	
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
			Console.WriteLine("conn="+env.ContentRootPath);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddOptions();
            services.Configure<ResultSettings>(Configuration.GetSection("ResultSettings"));

            services.AddMvc();
			Console.WriteLine("conn="+Environment.GetEnvironmentVariable("ConnectionString"));
            services.AddDbContext<Database>(opt=>opt.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionString")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
			app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
