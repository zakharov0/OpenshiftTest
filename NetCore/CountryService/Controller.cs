using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore; 
using System.Threading;  
using System.Threading.Tasks;  
using System.Text;
using Newtonsoft.Json;

namespace MicroService
{
    ///<summary>
    ///
    ///</summary>
    public class ErrorInfo
    {
        ///<summary>
        ///
        ///</summary>
        public string Message{get;set;}
        ///<summary>
        ///
        ///</summary>
        public string Exception{get;set;}

    }

    ///<summary>
    ///
    ///</summary>
    [Route("/")]
    public class HomeController : Controller
    { 
        ///<summary>
        ///       
        ///</summary>
        public IActionResult Index() 
        { 
            return Redirect("/swagger/Countries/ui");
        }
    }

    ///<summary>
    ///
    ///</summary>
    [Route("api/v1/[controller]")]
    public class CountriesController : Controller
    {
        ///<summary>
        ///
        ///</summary>
        ///<param name="query"></param>
        ///<param name="limit"></param>
        ///<param name="offset"></param>
        ///<returns></returns>  
        IActionResult ProcessQuery<T>(IQueryable<T> query, int limit, int offset)
        {
            if (!ModelState.IsValid)
            {
                var  el = new List<ErrorInfo>();
                foreach(var state in ModelState)
                    foreach(var e in state.Value.Errors)
                        if (e.Exception!=null)
                            el.Add(new ErrorInfo(){Exception=e.Exception.GetType().ToString(), Message=e.Exception.Message});
                        else
                            el.Add(new ErrorInfo(){Exception=null, Message=e.ErrorMessage});
                return new BadRequestObjectResult(el);
            }

            try
            {
                return Ok(query.Skip(offset).Take(limit).ToArray());
            }
            catch(Exception e){                
                Response.StatusCode = 500;
                return new ObjectResult(new ErrorInfo[]{ new ErrorInfo(){Exception=e.GetType().ToString(), Message=e.Message} });
            }
        }
		
        private readonly Database _context;		
        //private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _redis;
        private readonly List<Country> _repo;

        ///<summary>
        ///
        ///</summary>
        ///<param name="context"></param>	
        ///<param name="repo"></param>
		public CountriesController(Database context, /* Microsoft.Extensions.Caching.Distributed.IDistributedCache redis, */ List<Country> repo)
        {
            _context = context;
            //_redis = redis;
            _repo = repo;
        }

        ///<summary>
        /// Selects all countries
        ///</summary>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of countries</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(Country[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("All/{limit:int?}/{offset:int?}")]
        public IActionResult Get([FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
           return ProcessQuery(_repo.OrderBy(c=>c.name).ThenBy(c=>c.country_id).AsQueryable(), limit, offset);
        }

        ///<summary>
        /// Selects a specific country
        ///</summary>
        ///<remarks>
        /// Test uuid=f8541027-4f68-4ad3-8ef7-29d04ba06189
        ///</remarks>
        ///<param name="uuid">country ID</param>
        ///<returns>An array of the only country instance or nothing</returns> 
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        // <response code="408">Request Timeout</response>
        [HttpGet("{uuid}")]
        [ProducesResponseType(typeof(Country[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public IActionResult Get([FromRoute]Guid uuid)
        { 
            return ProcessQuery(_repo.Where(c=>c.country_id==uuid).AsQueryable(), 1, 0);
        }        

        ///<summary>
        /// Searches country by name
        ///</summary>
        ///<param name="name">name search pattern</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of countries</returns>   
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(Country[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("Search/{name}/{limit:int?}/{offset:int?}")]
        public IActionResult Search(string name, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            return ProcessQuery(_repo.Where(c=>c.name!=null && c.name.ToLower().Contains(name.ToLower())).OrderBy(c=>c.name).ThenBy(c=>c.country_id).AsQueryable(), limit, offset);
        }
        
        ///<summary>
        /// Searches countries by code
        ///</summary>
        ///<param name="code">country code prefix pattern</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of countries</returns>   
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(Country[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("ByCode/{code}/{limit:int?}/{offset:int?}")]
        public IActionResult Search(int code, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        { 
            return ProcessQuery(_repo.Where(v=>v.flag_code.ToString().StartsWith(code.ToString())).OrderBy(c=>c.name).ThenBy(c=>c.country_id).AsQueryable(), limit, offset);
        }


        private const int MAX_PAGE_SIZE = 50;

        ///<summary>
        /// Selects countries by codes
        ///</summary>
        ///<remarks>
        /// Test codes=[219,319]
        ///</remarks>
        ///<param name="codes">array of country codes</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of countries</returns>   
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(Country[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpPost("ByCode/{limit:int?}/{offset:int?}")]
        public IActionResult Search([FromBody]int[] codes, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        { 
            if (ModelState.IsValid)
            {
                string eps = Environment.GetEnvironmentVariable("PAGE_SIZE");
                int ps = String.IsNullOrEmpty(eps) ? MAX_PAGE_SIZE : int.Parse(eps);
                if (limit<=0 || limit>ps)
                    limit = ps;
            }
            var a = codes.Skip(offset).Take(limit);
            return ProcessQuery(_repo.Where(v=>v.flag_code!=null && a.Contains((int)v.flag_code)).AsQueryable(), limit, 0);
        }


        ///<summary>
        /// Inserts or updates country
        ///</summary>
        ///<remarks>
        /// Stub (out of funvtion)
        ///</remarks>
        ///<param name="data">country data</param>
        /// <response code="201">A newly created country object</response>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        [HttpPut]
        [ProducesResponseType(typeof(Country[]), 201)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Put([FromBody]Country data)
        {                   
            if (ModelState.IsValid && (data==null))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10); 
                return new NoContentResult();
            }
            else
            {
                var  el = new List<ErrorInfo>();
                foreach(var state in ModelState)
                    foreach(var e in state.Value.Errors)
                        if (e.Exception!=null)
                            el.Add(new ErrorInfo(){Exception=e.Exception.GetType().ToString(), Message=e.Exception.Message});
                        else
                            el.Add(new ErrorInfo(){Exception=null, Message=e.ErrorMessage});
                return new BadRequestObjectResult(el);
            }
        }

        ///<summary>
        /// Inserts or updates countries
        ///</summary>
        ///<remarks>
        /// Stub (out of funvtion)
        ///</remarks>
        ///<param name="data">countries data</param>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        // <response code="201">Returns the newly created item</response>
        // POST api/values
        [HttpPost]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Post([FromBody]Country[] data)
        {  
            if (ModelState.IsValid && (data==null || data.Length==0))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10); 
                return new NoContentResult();
            }
            else
            {
                var  el = new List<ErrorInfo>();
                foreach(var state in ModelState)
                    foreach(var e in state.Value.Errors)
                        if (e.Exception!=null)
                            el.Add(new ErrorInfo(){Exception=e.Exception.GetType().ToString(), Message=e.Exception.Message});
                        else
                            el.Add(new ErrorInfo(){Exception=null, Message=e.ErrorMessage});
                return new BadRequestObjectResult(el);
            }
        }

        ///<summary>
        /// Deletes countries
        ///</summary>
        ///<remarks>
        /// Stub (out of funvtion)
        ///</remarks>
        ///<param name="uuids">an array of country IDs</param>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        [HttpDelete]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Delete([FromBody]Guid[] uuids)
        {             
            if (ModelState.IsValid && (uuids==null || uuids.Length==0))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10);
                return new NoContentResult();            
            }
            else
            {
                var  el = new List<ErrorInfo>();
                foreach(var state in ModelState)
                    foreach(var e in state.Value.Errors)
                        if (e.Exception!=null)
                            el.Add(new ErrorInfo(){Exception=e.Exception.GetType().ToString(), Message=e.Exception.Message});
                        else
                            el.Add(new ErrorInfo(){Exception=null, Message=e.ErrorMessage});
                return new BadRequestObjectResult(el);
            }
        }

    }
}
