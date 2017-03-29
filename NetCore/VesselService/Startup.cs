using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.EntityFrameworkCore;

namespace VesselService
{
	
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
            //services.AddOptions();
            //services.Configure<ResultSettings>(Configuration.GetSection("ResultSettings"));

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
