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
        ///<summary>
        ///
        ///</summary>
        ///<param name="query"></param>
        ///<param name="limit"></param>
        ///<param name="offset"></param>
        ///<returns></returns> 
        async Task<IActionResult> ProcessQueryAsync<T>(IQueryable<T> query, int limit, int offset)
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

            Exception ex = null;
            try
            {
                int page_size = int.Parse(Environment.GetEnvironmentVariable("PAGE_SIZE"));
                if (limit<=0 || limit>page_size)
                    limit = page_size;

                var t = Task.Run(()=>{
                    try{        
                        return query.Skip(offset).Take(limit).ToArray();   
                        //var a = query.Skip(offset).Take(limit).ToArray();   
                        //foreach(var v in a)
                            //Console.WriteLine(v);
                        //return a;                      
                    }
                    catch(Exception)  
                    {   
                        Console.WriteLine("TRY REPEAT");
                        var startwatch = DateTime.Now;
                        try{
                            return  query.Skip(offset).Take(limit).ToArray(); 
                        }
                        catch(Exception e)  
                        {  
                            if ((DateTime.Now-startwatch).TotalSeconds>_context.Database.GetCommandTimeout()) 
                            {
                                Console.WriteLine("TIME OUT EXPIRED "+_context.Database.GetCommandTimeout());
                                throw new TimeoutException(e.Message);
                            }
                            else
                                throw e;
                        }
                    }                     
                });
                var rv = await t;
                return Ok(rv);
            }
            catch(TimeoutException e){                                      
                Response.StatusCode = 408;
                ex = e;
            }
            catch(Exception e){            
                Response.StatusCode = 500;
                ex = e;
            }
            return new ObjectResult(new ErrorInfo[]{ new ErrorInfo(){Exception=ex.GetType().ToString(), Message=ex.Message} });
        }

		
        private readonly Database _context;

        ///<summary>
        ///
        ///</summary>
        ///<param name="context"></param>	
		public VesselPositionsController(Database context)//
        {
            _context = context;
            var timeout = 4;
            var envto = Environment.GetEnvironmentVariable("COM_TIMEOUT");
            if (!String.IsNullOrEmpty(envto))
                timeout = int.Parse(envto);
            _context.Database.SetCommandTimeout(timeout); 
        }


        ///<summary>
        /// Selects single vessel positions in a date interval
        ///</summary>
        ///<param name="vessel_id">a vessel ID</param>
        ///<param name="start">a start date and time (UTC)</param>
        ///<param name="end">a finish date and time (UTC)</param>
        ///<param name="limit">a number of results displayed</param>
        ///<param name="offset">a start of results displayed</param>
        ///<returns>An array of vessel positions</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Supposedly invalid input date parameters</response>
        /// <response code="408">Timeout expired</response>
        [ProducesResponseType(typeof(VesselPosition[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [HttpGet("{vessel_id}/{start:datetime}/{end:datetime}/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Get(Guid vessel_id, [FromRoute]DateTime start, [FromRoute]DateTime end, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            if (ModelState.IsValid)
            {
                if  (start.TimeOfDay.TotalSeconds==0)
                    start = start.Date.AddDays(1);
                if (end>start)
                    ModelState.AddModelError("Input parameters", "An value of end precedes a value of start"); 
                Console.WriteLine(start + "" + start.Kind + " - " + end + "" + end.Kind);
            }
            return await ProcessQueryAsync(
            _context.VesselPositions.Include("NavStatus").Where(
                v=>v.vessel_id==vessel_id && v.ts_pos_utc<=start && v.ts_pos_utc>=end)
                .OrderByDescending(v=>v.ts_pos_utc)
            , limit, offset);
        }  

 
        ///<summary>
        /// Selects vessels nearby withinin a target radius
        ///</summary>
        ///<param name="vessel_id">a vessel ID</param>
        ///<param name="radius">the radius of a search area in kilometers</param>
        ///<param name="timespan">a time interval in minutes</param>
        ///<param name="limit">a number of results displayed</param>
        ///<param name="offset">a start of results displayed</param>
        ///<returns>An array of vessel positions</returns>  
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Path not found. Supposedly invalid input date parameters</response>
        /// <response code="408">Timeout expired</response>
        [ProducesResponseType(typeof(VesselPosition[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [HttpGet("Nearby/{vessel_id}/{radius:int?}/{timespan:int?}/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Nearby(Guid vessel_id, [FromRoute]int radius=50, [FromRoute]int? timespan=10, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        { 

            return await ProcessQueryAsync(
            _context.VesselPositions
//             .FromSql(@" select *
//  from ""VesselState"" 
// where state_id=60904595") 
            .FromSql(@"with 
""Target"" as (
    select wkb_geometry::geography point, ST_Buffer(ST_Point(157.069248333333,49.998106666666)::geography, " + (radius*1000) + @") buffer 
    from ""VesselState"" 
    where state_id=60904595),
""LatestPostions"" as (
select vessel_id, max(state_id) state_id
from ""VesselState"" v2 where state_id!=60904595 and 
ST_Covers( ST_SetSRID((select buffer from ""Target"" limit 1)::geometry, 0), wkb_geometry ) 
and ts_pos_utc>='2016-10-16 5:29'::timestamp and  ts_pos_utc<='2016-10-16 6:33' 
group by vessel_id)
select 
degrees(ST_Azimuth((select point from ""Target"" limit 1), wkb_geometry)) bearing,
ST_Distance((select point from ""Target"" limit 1)::geography, wkb_geometry) distance,
vp.* from ""LatestPostions"" lp, ""VesselState"" vp where lp.state_id=vp.state_id order by vp.ts_pos_utc desc")
                //.OrderByDescending(v=>v.ts_pos_utc)
            , limit, offset);
        }             
/*
        ///<summary>
        /// Searches vessels by name
        ///</summary>
        ///<param name="name">a name pattern</param>
        ///<param name="limit">a number of results displayed</param>
        ///<param name="offset">a start of results displayed</param>
        ///<returns>An array of vessels</returns>   
        /// <response code="400">Invalid input parameters</response>
        [ProducesResponseType(typeof(Vessel[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [HttpGet("Search/{name}/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Search(string name, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            return await ProcessQueryAsync(
            _context.Vessel.Where(v=>v.vessel_name.Contains(name.ToUpper())).OrderBy(v=>v.vessel_id)
            , limit, offset);
        }

        ///<summary>
        /// Advanced vessels search
        ///</summary>
        ///<param name="query">query parameters</param>
        ///<param name="limit">a number of results displayed</param>
        ///<param name="offset">a start of results displayed</param>
        ///<returns>An array of vessels</returns>   
        /// <response code="400">Invalid input parameters</response>
        [ProducesResponseType(typeof(Vessel[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [HttpPost("Search/{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Search([FromBody]VesselQuery query, [FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            var q = _context.Vessel.AsQueryable();
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
            return await ProcessQueryAsync(
            q.OrderBy(v=>v.vessel_id), limit, offset);
        }

        ///<summary>
        /// Selects a specific vessel
        ///</summary>
        ///<remarks>
        /// The example: api/v1/Vessels/a21c6578-6281-48ad-8328-367456661e1a
        ///</remarks>
        ///<param name="uuid">a vessel identificator</param>
        ///<returns>The vessel instance</returns> 
        /// <response code="400">Invalid input parameters</response>
        // <response code="408">Request Timeout</response>
        [HttpGet("{uuid}")]
        [ProducesResponseType(typeof(Vessel[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        public async Task<IActionResult> Get([FromRoute]Guid uuid)
        { 
            return await ProcessQueryAsync(_context.Vessel.Where(v=>v.vessel_id==uuid), 1, 0);
        }

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
