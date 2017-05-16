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
        ///<summary>


        Country _flag_country;
        ///<summary>
        ///
        ///</summary> 
        [Required]                  
        public Country flag_country{get{return _flag_country;}}

        ///<summary>
        ///
        ///</summary>
        [Required]
        public int? vessel_type_code{get;set;}


        ///<summary>
        ///
        ///</summary>
        public static async void SetCountry(object[] vessels)
        {
            string stringData;
            using (HttpClient client = new HttpClient())
            {
                var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                client.DefaultRequestHeaders.Accept.Add(contentType);
                HttpResponseMessage response = await client.PostAsync(Environment.GetEnvironmentVariable("COUNTRY_SRV"), 
                new StringContent("["+String.Join("", vessels.Select(v=>v.flag_code))+"]", System.Text.Encoding.UTF8, "application/json"));
                stringData = await response.Content.ReadAsStringAsync();
                Console.Write(stringData);
                var countries = JsonConvert.DeserializeObject<Country[]>(stringData);
                foreach (var v in vessels)
                {
                    v._flag_country = countries.FirstOrDefault(c=>c.flag_code==v.flag_code);
                }
            }
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
}