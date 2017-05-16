using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace MicroService
{
    ///<summary>
    ///
    ///</summary>
    [XmlType("VesselType")]
    public class VesselType
    {       
        ///<summary>
        ///
        ///</summary>
        [Required]
        public int? vessel_type_code{get;set;}
        ///<summary>
        ///
        ///</summary>
        [Required]
        public string vessel_type{get;set;}

        ///<summary>
        ///
        ///</summary>
        public VesselType()
        {

        }

        ///<summary>
        ///
        ///</summary>
        public VesselType(System.Data.IDataReader r)
        {
            //country_id = (Guid)r["country_id"];
            vessel_type_code = (int?)r["vessel_type_code"];
            vessel_type = (string)r["vessel_type"];    
       }    
 
        ///<summary>
        ///
        ///</summary>
        public override string ToString()
        {
            return String.Format("vessel_type_code={0}, vessel_type={1}", vessel_type_code, vessel_type);
        }        
    }
}