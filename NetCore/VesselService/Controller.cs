using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore; 
using System.Threading;  
using System.Threading.Tasks;

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
            return Redirect("/swagger/Vessels/ui");
        }
    }

    ///<summary>
    ///
    ///</summary>
    [Route("api/v1/[controller]")]
    public class VesselsController : Controller
    {
  
        private const int MAX_TIMEOUT = 3;
        private const int MAX_PAGE_SIZE = 50;

        ///<summary>
        ///
        ///</summary>
        ///<param name="func"></param>
        ///<param name="limit"></param>
        ///<param name="offset"></param>
        ///<returns></returns> 
        async Task<IActionResult> ProcessQueryAsync<T>(Func<Database, IQueryable<T> > func, int limit, int offset)
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
            Exception ex = null; 
            var timeout = MAX_TIMEOUT;
            var eto = Environment.GetEnvironmentVariable("CMD_TIMEOUT");
            if (!String.IsNullOrEmpty(eto))
                timeout = int.Parse(eto);           

            Func<T[]> func1 = ()=>{
                    using ( var db = new Database(Environment.GetEnvironmentVariable("CONNECTION_STRING")))
                    {
                        db.Database.SetCommandTimeout(timeout); 
                        var query = func(db);
                        Console.WriteLine("RUN '"+Thread.CurrentThread.ManagedThreadId+"'");
                        var startwatch = DateTime.Now;
                        try{
                            var a = query.Skip(offset).Take(limit).ToArray();
                            Console.WriteLine("DONE '"+Thread.CurrentThread.ManagedThreadId+"'");
                            return a;          
                        }
                        catch(Exception e)  
                        {  
                            if ((DateTime.Now-startwatch).TotalSeconds > timeout) 
                            {
                                Console.WriteLine("TIME OUT EXPIRED " + timeout);
                                throw new TimeoutException(e.Message);
                            }
                            else
                            {
                                Console.WriteLine("EXCEPTION OCCURED "+e.Message+" "+e.GetType()+" '"+Thread.CurrentThread.ManagedThreadId+"'");
                                throw e;
                            }
                        }    
                    }           
            };     

            try
            {
                var task = Task.Run(func1);
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout * 1000 + 50));
                if (completedTask==task)
                {
                    var rv = await task;
                    return Ok(rv);
                }
                else
                {
                    var rv = await Task.Run(func1);
                    return Ok(rv);
                }              
            }
            catch(TimeoutException e){                                      
                Response.StatusCode = 408;
                ex = e;
            }
            catch(Exception e){            
                Response.StatusCode = 500;
                ex = e;
            }
            finally
            {
                //db.Dispose();
            }

            return new ObjectResult(new ErrorInfo[]{ new ErrorInfo(){Exception=ex.GetType().ToString(), Message=ex.Message} });
        }
		
        private readonly Database _context;

        ///<summary>
        ///
        ///</summary>
        ///<param name="context"></param>	
		public VesselsController(Database context)//
        {
            _context = context;
            var timeout = MAX_TIMEOUT;
            var envto = Environment.GetEnvironmentVariable("COM_TIMEOUT");
            if (!String.IsNullOrEmpty(envto))
                timeout = int.Parse(envto);
            _context.Database.SetCommandTimeout(timeout); 
        }

        ///<summary>
        /// Selects a specific vessel
        ///</summary>
        ///<remarks>
        /// Test it uuid=a21c6578-6281-48ad-8328-367456661e1a
        ///</remarks>
        ///<param name="uuid">vessel identificator</param>
        ///<returns>The vessel instance</returns> 
        /// <response code="400">Invalid input parameters</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [HttpGet("{uuid}")]
        [ProducesResponseType(typeof(Vessel[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Get([FromRoute]Guid uuid)
        { 
            return await ProcessQueryAsync((context)=>{
                return context.Vessel.Where(v=>v.vessel_id==uuid);
            }, 1, 0);
        }

        ///<summary>
        /// Selects all vessels
        ///</summary>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">start of results displayed (0 based)</param>
        ///<returns>An array of vessels</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(Vessel[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Get([FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            return await ProcessQueryAsync((context)=>{
                return context.Vessel.OrderBy(v=>v.vessel_id);
            }, limit, offset);
        }

        ///<summary>
        /// Searches vessels by name
        ///</summary>
        ///<param name="name">name pattern</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">start of results displayed (0 based)</param>
        ///<returns>An array of vessels</returns>   
        /// <response code="400">Invalid input parameters</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(Vessel[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("Search/{name}/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Search(string name, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            return await ProcessQueryAsync((context)=>{
                return context.Vessel.Where(v=>v.vessel_name.Contains(name.ToUpper())).OrderBy(v=>v.vessel_id);
            }, limit, offset);
        }

        ///<summary>
        /// Advanced vessels search
        ///</summary>
        ///<remarks>
        /// Test it query={callsign:"UBBE4", mmsi:273352160}
        ///</remarks>
        ///<param name="query">query parameters</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">start of results displayed (0 based)</param>
        ///<returns>An array of vessels</returns>   
        /// <response code="400">Invalid input parameters</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(Vessel[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpPost("Search/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Search([FromBody]VesselQuery query, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        { 
            return await ProcessQueryAsync((context)=>{            
                var q = context.Vessel.AsQueryable();
                if (ModelState.IsValid)
                {
                    if (query.IsEpmty())   
                        ModelState.AddModelError("Input parameters", "Empty query"); 
                    if (query.imo.HasValue)
                        q = q.Where(v=>v.imo!=null && v.imo==query.imo);
                    if (query.mmsi.HasValue)
                        q = q.Where(v=>v.mmsi!=null && v.mmsi==query.mmsi);
                    if (!String.IsNullOrEmpty(query.vessel_name) && query.vessel_name.Length>1)
                        q = q.Where(v=>v.vessel_name!=null && v.vessel_name.Contains(query.vessel_name.ToUpper().ToString()));
                    if (!String.IsNullOrEmpty(query.callsign) && query.callsign.Length>1)
                        q = q.Where(v=>v.callsign!=null && v.callsign.Contains(query.callsign.ToUpper().ToString()));
                    if (query.flag_code.HasValue)
                        q = q.Where(v=>v.flag_code!=null && v.flag_code==query.flag_code);
                    if (query.vessel_type_code.HasValue)
                        q = q.Where(v=>v.vessel_type_code!=null && v.vessel_type_code==query.vessel_type_code);
                } 
                return q.OrderBy(v=>v.vessel_id);
            }, limit, offset);
        }

        ///<summary>
        /// Inserts or updates vessels
        ///</summary>
        ///<remarks>
        /// Stub. Out of function.
        ///</remarks>
        ///<param name="data">an array of vessels data</param>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        // <response code="201">Returns the newly created item</response>
        // POST api/values
        [HttpPost]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Post([FromBody]Vessel[] data)
        {  
            if (ModelState.IsValid && (data==null || data.Length==0))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10);
                foreach(var v in data)
                    Console.WriteLine(v + " UPDATED");
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
        ///<remarks>
        /// Stub. Out of function.
        ///</remarks>
        ///<param name="data">the vessel data</param>
        /// <response code="201">A newly created vessel object</response>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        [HttpPut]
        [ProducesResponseType(typeof(Vessel[]), 201)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Put([FromBody]Vessel data)
        {                   
            if (ModelState.IsValid && (data==null))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10); 
                if (data.vessel_id.HasValue)
                { 
                    Console.WriteLine(data + " UPDATED");
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
        /// Stub. Out of function.
        /// Test it &lt;ArrayOfGuid&gt;&lt;guid&gt;a21c6578-6281-48ad-8328-367456661e1a&lt;/guid&gt;&lt;/ArrayOfGuid&gt;
        ///</remarks>
        ///<param name="uuids">an array of vessel identificators</param>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        //[HttpDelete("{id}")]
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
    }
}
