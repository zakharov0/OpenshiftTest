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

            // if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDOC_HELP")))
            //     throw new ArgumentNullException("XDOC_HELP");
            // if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONNECTION_STRING")))
            //     throw new ArgumentNullException("CONNECTION_STRING");
            // if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PAGE_SIZE")))
            //     throw new ArgumentNullException("PAGE_SIZE");

            services.AddMvc(
                options =>
                    { 
                        //options.RespectBrowserAcceptHeader = true; 
                        //options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                        options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                        options.InputFormatters.Add(new XmlSerializerInputFormatter());
                        //options.AddJsonpOutputFormatter();
                    }
            );

            services.AddCors(options => 
            {
                options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            });

			Console.WriteLine("CONN="+Environment.GetEnvironmentVariable("CONNECTION_STRING"));
            services.AddDbContext<Database>(opt=>opt.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));

/*            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = "CountryCache";
                options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONFIG");
            });
*/

            services.AddSingleton<List<Country>>(); 

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
                    // 
                    options.IncludeXmlComments(Environment.GetEnvironmentVariable("XDOC_HELP"));
                }               
            );

        }

        ///<summary>
        ///
        ///</summary>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDistributedCache redis, Database db, List<Country> repo)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
			app.UseStaticFiles();

            app.UseCors("AllowAll");

            app.UseMvc();
  
            app.UseSwagger("swagger/api/{apiVersion}/Country.json");
            app.UseSwaggerUi("swagger/Country/ui", "/swagger/v1/Country.json");

            var countries = db.Country.OrderBy(c=>c.country_id).ToArray<Country>();   
            foreach(var country in countries) 
                repo.Add(country);
/*            foreach(var country in countries) 
            {
                redis.Set(country.country_id.ToString(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(country)));
            }             
            redis.Set("CountryIds", Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(countries.OrderBy(c=>c.country_id).Select(c=>c.country_id).ToArray())));            
            redis.Set("CountryCodes", Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(countries.OrderBy(c=>c.country_id).Select(c=>c.flag_code).ToArray()))); 
            redis.Set("CountryNames", Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(countries.OrderBy(c=>c.country_id).Select(c=>c.name).ToArray())));
            Console.WriteLine("REDIS IN");
*/
            
        }
    }
}
