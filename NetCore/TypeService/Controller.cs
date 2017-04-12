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
            return Redirect("/swagger/VesselType/ui");
        }
    }

    ///<summary>
    ///
    ///</summary>
    [Route("api/v1/[controller]")]
    public class VesselTypeController : Controller
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
        private readonly List<VesselType> _repo;

        ///<summary>
        ///
        ///</summary>
        ///<param name="context"></param>	
        ///<param name="repo"></param>
		public VesselTypeController(Database context, List<VesselType> repo)
        {
            _context = context;
            _repo = repo;
        }

        ///<summary>
        /// Selects all types
        ///</summary>
        ///<param name="limit">a number of results displayed</param>
        ///<param name="offset">a start of results displayed</param>
        ///<returns>An array of vessel tyes</returns>  
        /// <response code="400">Invalid input parameters</response>
        [ProducesResponseType(typeof(VesselType[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [HttpGet("GetAll/{limit:int?}/{offset:int?}")]
        public IActionResult Get([FromRoute]int limit=-1, [FromRoute]int offset=0) 
        { 
            return ProcessQuery(_repo.OrderBy(c=>c.vessel_type).ThenBy(c=>c.vessel_type_code).AsQueryable(), limit, offset);
        }

        ///<summary>
        /// Selects a specific vessel type
        ///</summary>
        ///<param name="code">a type identificator</param>
        ///<returns>An array of the only vessel type instance or nothing</returns> 
        /// <response code="400">Invalid input parameters</response>
        // <response code="408">Request Timeout</response>
        //<remarks>
        // The path example: api/v1/Country/f8541027-4f68-4ad3-8ef7-29d04ba06189
        //</remarks>
        [HttpGet("{code}")]
        [ProducesResponseType(typeof(VesselType[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        public IActionResult Get(int code)
        { 
            return ProcessQuery(_repo.Where(c=>c.vessel_type_code==code).AsQueryable(), 1, 0);
        }        

        ///<summary>
        /// Searches vessel type by name
        ///</summary>
        ///<param name="name">a name search pattern</param>
        ///<param name="limit">a number of results displayed</param>
        ///<param name="offset">a start of results displayed</param>
        ///<returns>An array of vessel types</returns>   
        /// <response code="400">Invalid input parameters</response>
        [ProducesResponseType(typeof(VesselType[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [HttpGet("Search/{name}/{limit:int?}/{offset:int?}")]
        public IActionResult Search(string name, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            return ProcessQuery(_repo.Where(c=>c.vessel_type!=null && c.vessel_type.ToLower().Contains(name.ToLower()))
            .OrderBy(c=>c.vessel_type).ThenBy(c=>c.vessel_type_code).AsQueryable(), limit, offset);
        }
        
/*
        ///<summary>
        /// Inserts or updates vessels
        ///</summary>
        ///<param name="data">an array of vessels data</param>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="415">An unsupported media type</response>
        // <response code="201">Returns the newly created item</response>
        // POST api/values
        [HttpPost]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        public async Task<IActionResult> Post([FromBody]Vessel[] data)
        {  
            if (ModelState.IsValid && (data==null || data.Length==0))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10);
                foreach(var v in data)
                    Console.WriteLine(v + "UPDATED");
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
        /// Inserts or updates vessel
        ///</summary>
        ///<param name="data">the vessel data</param>
        /// <response code="201">A newly created vessel object</response>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="415">An unsupported media type</response>
        [HttpPut]
        [ProducesResponseType(typeof(Vessel[]), 201)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        public async Task<IActionResult> Put([FromBody]Vessel data)
        {                   
            if (ModelState.IsValid && (data==null))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10); 
                if (data.vessel_id.HasValue)
                { 
                    Console.WriteLine(data + "UPDATED");
                    return new NoContentResult();
                }
                else
                {               
                    Response.StatusCode = 201;
                    data.vessel_id = Guid.NewGuid();
                    return new ObjectResult(new Vessel[]{data});
                }
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
        /// Deletes vessels
        ///</summary>
        ///<remarks>
        /// The example: &lt;ArrayOfGuid&gt;&lt;guid&gt;a21c6578-6281-48ad-8328-367456661e1a&lt;/guid&gt;&lt;/ArrayOfGuid&gt;
        ///</remarks>
        ///<param name="uuids">an array of vessel identificators</param>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="415">An unsupported media type</response>
        //[HttpDelete("{id}")]
        [HttpDelete]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        public async Task<IActionResult> Delete([FromBody]Guid[] uuids)
        {             
            if (ModelState.IsValid && (uuids==null || uuids.Length==0))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10);
                foreach(var uuid in uuids)
                    Console.WriteLine(uuid + " DELETED");
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
*/
    }
}
