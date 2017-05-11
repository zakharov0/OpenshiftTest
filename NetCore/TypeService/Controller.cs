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
            return Redirect("/swagger/VesselTypes/ui");
        }
    }

    ///<summary>
    ///
    ///</summary>
    [Route("api/v1/[controller]")]
    public class VesselTypesController : Controller
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

            string eps = Environment.GetEnvironmentVariable("PAGE_SIZE");
            int ps = String.IsNullOrEmpty(eps) ? MAX_PAGE_SIZE : int.Parse(eps);
            if (limit<=0 || limit>ps)
                limit = ps;

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
        private readonly List<VesselType> _repo;

        ///<summary>
        ///
        ///</summary>
        ///<param name="context"></param>	
        ///<param name="repo"></param>
		public VesselTypesController(Database context, List<VesselType> repo)
        {
            _context = context;
            _repo = repo;
        }

        ///<summary>
        /// Selects all types
        ///</summary>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of vessel tyes</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(VesselType[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("All/{limit:int?}/{offset:int?}")]
        public IActionResult Get([FromRoute]int limit=-1, [FromRoute]int offset=0) 
        { 
            return ProcessQuery(_repo.OrderBy(c=>c.vessel_type).ThenBy(c=>c.vessel_type_code).AsQueryable(), limit, offset);
        }

        ///<summary>
        /// Selects a specific vessel type
        ///</summary>
        ///<param name="code">type code</param>
        ///<returns>A vessel type</returns> 
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        [HttpGet("{code}")]
        [ProducesResponseType(typeof(VesselType[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public IActionResult Get(int code)
        { 
            return ProcessQuery(_repo.Where(c=>c.vessel_type_code==code).AsQueryable(), 1, 0);
        } 

        private const int MAX_PAGE_SIZE = 50;

        ///<summary>
        /// Selects vessel type
        ///</summary>
        ///<param name="codes">array of type codes</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>Vessel types</returns> 
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        [HttpPost("{limit:int?}/{offset:int?}")]
        [ProducesResponseType(typeof(VesselType[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public IActionResult Get([FromBody]int[] codes, [FromRoute]int limit=-1, [FromRoute]int offset=0)
        { 
            if (ModelState.IsValid)
            {
                string eps = Environment.GetEnvironmentVariable("PAGE_SIZE");
                int ps = String.IsNullOrEmpty(eps) ? MAX_PAGE_SIZE : int.Parse(eps);
                if (limit<=0 || limit>ps)
                    limit = ps;
            }
            var a = codes.Skip(offset).Take(limit);
            return ProcessQuery(_repo.Where(v=>v.vessel_type_code!=null && a.Contains((int)v.vessel_type_code)).AsQueryable(), limit, 0);
        }                

        ///<summary>
        /// Searches vessel type by name
        ///</summary>
        ///<param name="name">name search pattern</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of vessel types</returns>   
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(VesselType[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("Search/{name}/{limit:int?}/{offset:int?}")]
        public IActionResult Search(string name, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            return ProcessQuery(_repo.Where(c=>c.vessel_type!=null && c.vessel_type.ToLower().Contains(name.ToLower()))
            .OrderBy(c=>c.vessel_type).ThenBy(c=>c.vessel_type_code).AsQueryable(), limit, offset);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////

        ///<summary>
        /// Inserts or updates vessel type
        ///</summary>
        ///<remarks>
        /// Stub (out of funvtion)
        ///</remarks>
        ///<param name="data">vessel type data</param>
        /// <response code="201">A newly created vessel type object</response>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        [HttpPut]
        [ProducesResponseType(typeof(VesselType[]), 201)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Put([FromBody]VesselType data)
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
        /// Inserts or updates vessel types
        ///</summary>
        ///<remarks>
        /// Stub (out of funvtion)
        ///</remarks>
        ///<param name="data">vessel types data</param>
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
        public async Task<IActionResult> Post([FromBody]VesselType[] data)
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
        /// Deletes vessel types
        ///</summary>
        ///<remarks>
        /// Stub (out of funvtion)
        ///</remarks>
        ///<param name="codes">an array of type codes</param>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        [HttpDelete]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Delete([FromBody]int[] codes)
        {             
            if (ModelState.IsValid && (codes==null || codes.Length==0))   
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
