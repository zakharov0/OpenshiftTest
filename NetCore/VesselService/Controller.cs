using System;
using System.Collections.Generic;
using System.Linq;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Options;
//using Microsoft.EntityFrameworkCore;
//using Npgsql;

namespace VesselService
{
    [Route("api/[controller]")]
    //[Route("api")]
    public class VesselController : Controller
    {
		
        private readonly Database _context;
		
		public VesselController(Database context)//
        {
            _context = context;
        }

		
        // GET api/values
        [HttpGet]
        public IEnumerable<object> Get()
        {
			Console.WriteLine("Limit="+Environment.GetEnvironmentVariable("PageSize"));
            return _context.VesselInfo.Skip(0).Take(10).ToArray();//return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value"+id;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
