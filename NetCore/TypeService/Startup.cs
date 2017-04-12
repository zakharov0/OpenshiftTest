using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MicroService
{
    ///<summary>
    ///
    ///</summary>	
    public class Startup
    {
        ///<summary>
        ///
        ///</summary>
        public Startup(IHostingEnvironment env)
        {
			Console.WriteLine("ROOT="+env.ContentRootPath);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        ///<summary>
        ///
        ///</summary>
        public IConfigurationRoot Configuration { get; }

        ///<summary>
        ///
        ///</summary>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {   
            services.AddMvc(
                options =>
                    { 
                        options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                        options.InputFormatters.Add(new XmlSerializerInputFormatter());
                    }
            );

            services.AddCors(options => 
            {
                options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            });

			Console.WriteLine("CONN="+Environment.GetEnvironmentVariable("CONNECTION_STRING"));
            services.AddDbContext<Database>(opt=>opt.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));

            services.AddSingleton<List<VesselType>>(); 

            services.AddSwaggerGen(
                options =>
                {
                    options.MultipleApiVersions(new Swashbuckle.Swagger.Model.Info[]
                    {
                        new Swashbuckle.Swagger.Model.Info
                        {
                            Version = "v2",
                            Title = "MARINET",
                            Description = "A RESTful API (version 2.0)"
                        },
                        new Swashbuckle.Swagger.Model.Info
                        {
                            Version = "v1",
                            Title = "MARINET",
                            Description = "A RESTful API (version 1.0)"
                        }
                    }, (description, version) =>
                    {
                        return description.RelativePath.Contains($"api/{version}");
                    });
                    options.IncludeXmlComments(Environment.GetEnvironmentVariable("XDOC_HELP"));
                }               
            );

        }

        ///<summary>
        ///
        ///</summary>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, 
        IDistributedCache redis, Database db, List<VesselType> repo)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
			app.UseStaticFiles();

            app.UseCors("AllowAll");

            app.UseMvc();
  
            app.UseSwagger("swagger/{apiVersion}/VesselType.json");
            app.UseSwaggerUi("swagger/VesselType/ui", "/swagger/v1/VesselType.json");

            var vestypes = db.VesselType.OrderBy(c=>c.vessel_type_code).ToArray<VesselType>();   
            foreach(var vt in vestypes) 
                repo.Add(vt);            
        }
    }
}
