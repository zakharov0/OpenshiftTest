using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;  

namespace MicroService
{

    ///<summary>
    ///
    ///</summary>
    [XmlType("Vessel")]
    public class Vessel
    {
        ///<summary>
        ///
        ///</summary>
        public Guid? vessel_id{get;set;}        
        ///<summary>
        ///
        ///</summary>
        [Required]
        public int? mmsi{get;set;}
        ///<summary>
        ///
        ///</summary>
        [Required]
        public int? imo{get;set;}
        ///<summary>
        ///
        ///</summary>
        [Required]
        public string vessel_name{get;set;}
        ///<summary>
        ///
        ///</summary>
        [Required]
        public string callsign{get;set;} 
        ///<summary>
        ///
        ///</summary> 
        [Required]                  
        public int? flag_code{get;set;}
 
        Country _flag_country;
        ///<summary>
        ///
        ///</summary>              
        public Country flag_country{get{return _flag_country;}}

        ///<summary>
        ///
        ///</summary>
        [Required]
        public int? vessel_type_code{get;set;}

        VesselType _vessel_type;
        ///<summary>
        ///
        ///</summary>              
        public VesselType vessel_type{get{return _vessel_type;}}

        VesselPosition _position;
        ///<summary>
        ///
        ///</summary>              
        public VesselPosition position{get{return _position;}}

        ///<summary>
        ///
        ///</summary>
        private static async System.Threading.Tasks.Task<T[]> MicroServiceRequestAsync<T>(string url, string post)
        {           
            using (HttpClient client = new HttpClient())
            {
                var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                client.DefaultRequestHeaders.Accept.Add(contentType);
                HttpResponseMessage response = await client.PostAsync(url, 
                new StringContent(post, System.Text.Encoding.UTF8, "application/json"));
                var stringData = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(">>>"+stringData);
                var array = JsonConvert.DeserializeObject<T[]>(stringData);
                return array;
            }
        }
        
        ///<summary>
        ///
        ///</summary>
        public static async System.Threading.Tasks.Task<string> SetDetailsAsync(object a, bool withPosition)
        {
            Vessel[] vessels = (Vessel[]) a;
            var post = "["+String.Join(",", vessels.Select(v=>v.flag_code))+"]";
            var task1 = MicroServiceRequestAsync<Country>(Environment.GetEnvironmentVariable("COUNTRY_SRV"), post);
            post  = "["+String.Join(",", vessels.Select(v=>v.vessel_type_code))+"]";
            var task2 = MicroServiceRequestAsync<VesselType>(Environment.GetEnvironmentVariable("VESTYPE_SRV"), post);
            Country[] countries = null;
            VesselType[] vestypes = null;
            VesselPosition[] positions = null; 
            if (withPosition)
            {
                post  = "["+String.Join(",", vessels.Select(v=>"'"+v.vessel_id+"'"))+"]";
                Console.WriteLine(">>>"+post);
                var task3 = MicroServiceRequestAsync<VesselPosition>(Environment.GetEnvironmentVariable("VESPOS_SRV"), post);
                System.Threading.Tasks.Task.WaitAll(task1, task2, task3);
                countries = await task1;
                vestypes = await task2;
                positions = await task3; 
            }
            else
            {
                System.Threading.Tasks.Task.WaitAll(task1, task2);
                countries = await task1;
                vestypes = await task2;
            }

            foreach (var v in vessels)
            {
                    v._flag_country = countries.FirstOrDefault(c=>c.flag_code==v.flag_code);
                    v._vessel_type = vestypes.FirstOrDefault(vt=>vt.vessel_type_code==v.vessel_type_code);
                    if (positions!=null)
                        v._position = positions.FirstOrDefault(p=>p.vessel_id==v.vessel_id);
            }
            return "";
        }


/*
        ///<summary>
        ///
        ///</summary>
        public Vessel()
        {

        }

        ///<summary>
        ///
        ///</summary>
        public Vessel(System.Data.IDataReader r)
        {
            vessel_id = (Guid)r["vessel_id"];
            mmsi = (int?)r["mmsi"];
            imo = (int?)r["imo"];
            vessel_name = (string)r["vessel_name"];
            callsign = (string)r["callsign"];              
            flag_code = (int?)r["flag_code"];
            vessel_type_code = (int?)r["vessel_type_code"];      
        }    
*/
        ///<summary>
        ///
        ///</summary>
        public override string ToString()
        {
            return String.Format("vessel_id={0}, mmsi={1}, imo={2}, vessel_name={3}", vessel_id, mmsi, imo, vessel_name);
        }        
    }

    ///<summary>
    ///
    ///</summary>
    public static class ArrayExt
    {
        ///<summary>
        ///
        ///</summary>
        public static async System.Threading.Tasks.Task<T[]> ResolveDetailsAsync<T>(this T[] a, bool withPosition)
        {
            //Console.WriteLine("ResolveJoins >>>>>"+a);
            await Vessel.SetDetailsAsync(a, withPosition);
            return a;
        }

    }
}