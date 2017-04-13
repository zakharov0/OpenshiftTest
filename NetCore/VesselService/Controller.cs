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
/*
        ///<summary>
        ///
        ///</summary>
        ///<param name="result"></param>
        ///<param name="status"></param>
        ///<returns></returns>
        object WrapResult<T>(T[] result, string status="ok")
        {
            if(Request.Headers.ContainsKey("accept") && 
            !Request.Headers["accept"].Any(h=>h.Contains("html")) && 
            Request.Headers["accept"].Any(h=>h.Contains("xml")))
                return new Data<T>(result);
            else
                return new Dictionary<string,object>(){{"status",status}, {"result", result}};
        }

        ///<summary>
        ///
        ///</summary>
        ///<param name="query"></param>
        ///<param name="limit"></param>
        ///<param name="offset"></param>
        ///<returns></returns>  
        async Task<object> ProcessQueryAsync<T>(IQueryable<T> query, int limit, int offset)
        {


            // var temp = _context.Database.GetDbConnection();
            // try
            // {
            //     var cts = new CancellationTokenSource();
            //     cts.CancelAfter(100);
            //     await temp.OpenAsync(cts.Token);
            //     Console.WriteLine(">>> CONNECTION1: " + temp.State);
            // }
            // catch (Exception) { }
            // finally
            // {
            //     Console.WriteLine(">>> CONNECTION2: " + temp.State);
            //     if (temp.State == System.Data.ConnectionState.Open)
            //         temp.Close();
            // }
  
            try
            {
                int page_size = int.Parse(Environment.GetEnvironmentVariable("PAGE_SIZE"));
                if (limit<=0 || limit>page_size)
                    limit = page_size;

                var cts = new CancellationTokenSource();
                cts.CancelAfter(1000);
                var rv = await query.Skip(offset).Take(limit).ToArrayAsync();
                return WrapResult(rv);
            }
            catch(Exception){
                try
                {
                    var rv = await query.Skip(offset).Take(limit).ToArrayAsync();
                    return WrapResult(rv);
                } 
                catch(Exception e)
                {
                    return WrapResult(new ErrorInfo[]{ new ErrorInfo(){Exception=e.GetType().ToString(), Message=e.Message} }, "error");
                }                       
            }


            // try {
            //     if (_page_size==0)
            //         _page_size = int.Parse(Environment.GetEnvironmentVariable("PAGE_SIZE"));
            //     if (limit<=0 || limit>_page_size)
            //         limit = _page_size;
            //     try{
            //         var rv = await query.Skip(offset).Take(limit).ToArrayAsync();
            //         return WrapResult(rv);
            //     }                          
            //     catch(Npgsql.NpgsqlException e)
            //     {
            //         if (e.InnerException is System.IO.IOException)
            //         {
            //             var rv = await query.Skip(offset).Take(limit).ToArrayAsync();
            //             return WrapResult(rv);
            //         }
            //         else
            //             throw e; 
            //     }
            // } 
            // catch(Exception e)
            // {
            //     return WrapResult(new ErrorInfo[]{ new ErrorInfo(){Exception=e.GetType().ToString(), Message=e.Message} }, "error");
            // }
 
        }
*/
        
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

            try
            {
                int page_size = int.Parse(Environment.GetEnvironmentVariable("PAGE_SIZE"));
                if (limit<=0 || limit>page_size)
                    limit = page_size;

                using (var cts = new CancellationTokenSource())
                {
                    var s = DateTime.Now;
                    int timeout = 10000;
                    cts.CancelAfter(timeout);

                    Task<T[]> task = query.Skip(offset).Take(limit).ToArrayAsync(cts.Token);
                    if (await Task.WhenAny(task, Task.Delay(timeout + 100, cts.Token)) == task)
                    {
                        Console.WriteLine("COMPLETED WITHIN TIMEOUT " + (DateTime.Now-s).TotalSeconds);
                        return Ok(task.Result);
                    }
                    else
                    {
                        Console.WriteLine("Timeout expired. TRY REPEAT REQUEST " + (DateTime.Now-s).TotalSeconds);
                        // if(_context.Database.GetDbConnection().State==System.Data.ConnectionState.Open)
                        // {
                        //     Console.WriteLine("CLOSE CONNECTION");
                        //     _context.Database.GetDbConnection().Close();
                        // }
                        var rv = await query.Skip(offset).Take(limit).ToArrayAsync();
                        return Ok(rv);
                    }
                }

            }
            catch(Exception e){                
                Response.StatusCode = 500;
                return new ObjectResult(new ErrorInfo[]{ new ErrorInfo(){Exception=e.GetType().ToString(), Message=e.Message} });
            }
        }

		
        private readonly Database _context;

        ///<summary>
        ///
        ///</summary>
        ///<param name="context"></param>	
		public VesselsController(Database context)//
        {
            _context = context;
        }

        ///<summary>
        /// Selects all vessels
        ///</summary>
        ///<param name="limit">a number of results displayed</param>
        ///<param name="offset">a start of results displayed</param>
        ///<returns>An array of vessels</returns>  
        /// <response code="400">Invalid input parameters</response>
        [ProducesResponseType(typeof(Vessel[]), 200)]
        [ProducesResponseType(typeof(ErrorInfo[]), 400)]
        [HttpGet("{limit:int?}/{offset:int?}")]
        public async Task<IActionResult> Get([FromRoute]int limit=-1, [FromRoute]int offset=0) 
        {  
            //_context.Database.SetCommandTimeout(1);
            return await ProcessQueryAsync(
            _context.Vessel.OrderBy(v=>v.vessel_id)
            , limit, offset);
        }

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
    }
}
