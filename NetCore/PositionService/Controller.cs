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
            return Redirect("/swagger/VesselPositions/ui");
        }
    }

    ///<summary>
    ///
    ///</summary>
    [Route("api/v1/[controller]")]
    public class VesselPositionsController : Controller
    {
  
        private const int MAX_TIMEOUT = 3;	

        private Database _context;

        ///<summary>
        ///
        ///</summary>
        ///<param name="context"></param>	
		public VesselPositionsController(Database context)//
        {
            _context = context;
            var timeout = MAX_TIMEOUT;
            var envto = Environment.GetEnvironmentVariable("COM_TIMEOUT");
            if (!String.IsNullOrEmpty(envto))
                timeout = int.Parse(envto);
            _context.Database.SetCommandTimeout(timeout); 
        }

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
		
       
        ///<summary>
        /// Get a vessel position by its ID
        ///</summary>
        ///<remarks>
        /// Test position_id=60905406
        ///</remarks>
        ///<param name="position_id">vessel position ID</param>
        ///<returns>vessel position instance</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(VesselPosition[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("{position_id}")]
        public async Task<IActionResult> Get(int position_id) 
        {  
            return await ProcessQueryAsync((context)=>{
                return context.VesselPositions.Include("nav_status")
                .Where(v=>v.position_id==position_id);
            }, 1, 0);
        }  
              
        ///<summary>
        /// Get vessel positions by their IDs
        ///</summary>
        ///<remarks>
        /// Test position_ids=[60905406,60905377,60904605,60903790]
        ///</remarks>
        ///<param name="position_ids">array of vessel position IDs</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of vessel positions</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(VesselPosition[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpPost("{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Get([FromBody]long[] position_ids, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {             
            if (ModelState.IsValid)
            {
                string eps = Environment.GetEnvironmentVariable("PAGE_SIZE");
                int ps = String.IsNullOrEmpty(eps) ? MAX_PAGE_SIZE : int.Parse(eps);
                if (limit<=0 || limit>ps)
                    limit = ps;
            }
            return await ProcessQueryAsync((context)=>{
                return context.VesselPositions.Include("nav_status")
                .Where(v=>position_ids.Skip(offset).Take(limit).Contains(v.position_id));
            }, limit, 0);
        }  

        ///<summary>
        /// Selects positions of a specific vessel in a time period
        ///</summary>
        ///<remarks>
        /// Test vessel_id=c12c4d21-5800-4af1-a7ab-b970f9f87819, start=2016-10-16, finish=2016-10-16 06:31:29
        ///</remarks>
        ///<param name="vessel_id">vessel ID</param>
        ///<param name="start">start of period (UTC)</param>
        ///<param name="finish">finish of period (UTC)</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of vessel positions</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(VesselPosition[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("{vessel_id}/{start:datetime}/{finish:datetime}/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Get(Guid vessel_id, [FromRoute]DateTime? start, [FromRoute]DateTime? finish, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            DateTime s = start ?? DateTime.Today, f = finish ?? DateTime.Today;
            if (ModelState.IsValid)
            {
                if  (f.TimeOfDay.TotalSeconds==0)
                    f = f.Date.AddDays(1).AddMilliseconds(-1);
                if (f<s)
                    ModelState.AddModelError("Input parameters", "A start moment must precede a finish moment"); 
                Console.WriteLine(s + "" + s.Kind + " - " + f + "" + f.Kind);
            }
            return await ProcessQueryAsync((context)=>{
                return context.VesselPositions.Include("nav_status")
                .Where(v=>v.vessel_id==vessel_id && s<=v.ts_pos_utc && v.ts_pos_utc<=f)
                .OrderByDescending(v=>v.ts_pos_utc);
            }, limit, offset);
        }   

        ///<summary>
        /// Selects a latest position of a vessel
        ///</summary>
        ///<remarks>
        /// Test vessel_id=c12c4d21-5800-4af1-a7ab-b970f9f87819
        ///</remarks>
        ///<param name="vessel_id">vessel ID</param>
        ///<returns>An array of vessel positions</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(VesselPosition[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("Last/{vessel_id}")]
        public async Task<IActionResult> GetLast(Guid vessel_id) 
        {  
            return await ProcessQueryAsync((context)=>{
                return context.VesselPositions.Include("nav_status")
                .Where(v=>v.vessel_id==vessel_id)
                .OrderByDescending(v=>v.ts_pos_utc);
            }, 1, 0);
        }  
 
        ///<summary>
        /// Selects latest positions of a set of vessels
        ///</summary>
        ///<remarks>
        /// Test vessel_ids=['42977b8c-7d0b-44e4-81d0-15bf4ce1338a','947ecba6-5c1b-4475-ba4f-b5fcb87d7904','79a5aae6-23a0-4e4b-99a1-3b99c4485ed0']
        ///</remarks>
        ///<param name="vessel_ids">vessel ID array</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of vessel positions</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(VesselPosition[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpPost("Last/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> GetLast([FromBody]Guid[] vessel_ids, [FromRoute]int limit=-1, [FromRoute]int offset=0)
        {             
            if (ModelState.IsValid)
            {
                string eps = Environment.GetEnvironmentVariable("PAGE_SIZE");
                int ps = String.IsNullOrEmpty(eps) ? MAX_PAGE_SIZE : int.Parse(eps);
                if (limit<=0 || limit>ps)
                    limit = ps;
            }

            var a = vessel_ids.Skip(offset).Take(limit).Select(uuid=>String.Format(@"select max(state_id) state_id from ""VesselState"" where vessel_id='{0}'", uuid));
            string sql = String.Format(@"
            select vp.* from ""VesselState"" vp, ({0}) mp
            where mp.state_id=vp.state_id
            ", String.Join(" union ", a));
            //Console.WriteLine(sql);

            return await ProcessQueryAsync((context)=>{
            return context.VesselPositions.FromSql(sql).Include("nav_status");
            }, limit, 0);
        }  

        private const int MAX_RADIUS = 100;
        private const int MAX_TIMESPAN = 60;

        ///<summary>
        /// Selects vessels nearby withinin a target radius
        ///</summary>
        ///<remarks>
        /// Test position_id=60904595, radius=40, timespan=30
        ///</remarks>
        ///<param name="position_id">vessel position ID nearby to search</param>
        ///<param name="radius">radius of search area in kilometers (max 100)</param>
        ///<param name="timespan">time interval in minutes (max 60)</param>
        ///<param name="limit">number of results displayed</param>
        ///<param name="offset">number of results skiped from start</param>
        ///<returns>An array of vessel positions</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="500">Request process failed on server</response>
        [ProducesResponseType(typeof(VesselRelativePosition[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        [HttpGet("Nearby/{position_id}/{radius:int?}/{timespan:int?}/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Nearby(long position_id, [FromRoute]int radius=-1, [FromRoute]int timespan=-1, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        { 
            if (ModelState.IsValid)
            {
                if (timespan<=0 || timespan>MAX_TIMESPAN)
                    timespan = MAX_TIMESPAN;
                if (radius<=0 || radius>MAX_RADIUS)
                    radius = MAX_RADIUS;
            }
            //60904595, 45
            return await ProcessQueryAsync((context)=>{
                var sql = String.Format(@"select {0} base_position_id,
degrees(ST_Azimuth((select wkb_geometry::geography from ""VesselState"" where state_id={0}), wkb_geometry)) bearing,
ST_Distance((select wkb_geometry::geography from ""VesselState"" where state_id={0})::geography, wkb_geometry) distance,
vp.*
from (
      select vessel_id, max(state_id) state_id
      from ""VesselState"" v2 where state_id!={0} and
      ST_Covers( ST_SetSRID((select ST_Buffer(ST_Point(longitude,latitude)::geography, {1}000) from ""VesselState"" where state_id={0})::geometry, 0), wkb_geometry )
      and ts_pos_utc>=(select ts_pos_utc from ""VesselState"" where state_id={0}) and 
      ts_pos_utc<=(select ts_pos_utc + interval '{2} minutes' from ""VesselState"" where state_id={0})
      group by vessel_id
) lp, ""VesselState"" vp
where lp.state_id=vp.state_id
order by vp.ts_pos_utc desc", position_id, radius, timespan);
                //Console.WriteLine(sql);
                return context.VesselRelativePositions 
                .FromSql(sql).Include("nav_status");
            }, limit, offset);
        }  
  
        ///<summary>
        /// Inserts or updates vessel position
        ///</summary>
        ///<remarks>
        /// Stub (out of function)
        ///</remarks>
        ///<param name="data">vessel position data</param>
        /// <response code="201">A newly created vessel position</response>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        [HttpPut]
        [ProducesResponseType(typeof(VesselPosition[]), 201)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Put([FromBody]VesselPosition data)
        {                   
            if (ModelState.IsValid && (data==null))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10); 
                if (_context.VesselPositions.Any(vp=>vp.position_id==data.position_id))
                { 
                    Console.WriteLine(data + "UPDATED");
                    return new NoContentResult();
                }
                else
                {               
                    Response.StatusCode = 201;
                    Console.WriteLine("LAST_ID " + _context.VesselPositions.Max(vp=>vp.position_id));
                    //data.position_id = Guid.NewGuid();
                    return new ObjectResult(new VesselPosition[]{data});
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
        /// Inserts or updates vessel positions
        ///</summary>
        ///<remarks>
        /// Stub (out of function)
        ///</remarks>
        ///<param name="data">array of vessel positions</param>
        /// <response code="201">A newly created vessel position</response>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        [HttpPost]
        [ProducesResponseType(typeof(VesselPosition[]), 201)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Post([FromBody]VesselPosition[] data)
        {                   
            if (ModelState.IsValid && (data==null))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10); 
                var newly = new List<VesselPosition>();
                foreach(var vespos in data)
                    if (_context.VesselPositions.Any(vp=>vp.position_id==vespos.position_id))
                    { 
                        Console.WriteLine(vespos + " UPDATED");
                    }
                    else
                    {  
                        newly.Add(vespos);
                        Console.WriteLine(vespos + " INSERTED");
                        //data.position_id = Guid.NewGuid();
                    }
                if (newly.Count>0)   
                {              
                    Response.StatusCode = 201;               
                    return new ObjectResult(newly.ToArray());
                }
                else
                {
                    return new NoContentResult();
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
        /// Deletes vessel positions
        ///</summary>
        ///<remarks>
        /// Stub (out of function)
        ///</remarks>
        ///<param name="ids">array of vessel position IDs</param>
        /// <response code="204">Success with a response having an enpty content</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Invalid request format</response>
        /// <response code="408">Timeout expired</response>
        /// <response code="415">An unsupported media type</response>
        /// <response code="500">Request process failed on server</response>
        [HttpDelete]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [ProducesResponseType(typeof(ErrorInfo[]), 500)]
        public async Task<IActionResult> Delete([FromBody]long[] ids)
        {             
            if (ModelState.IsValid && (ids==null || ids.Length==0))   
                ModelState.AddModelError("Input parameters", "Empty input");     
            if (ModelState.IsValid)
            {
                await Task.Delay(10);
                foreach(var uuid in ids)
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
