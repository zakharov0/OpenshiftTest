using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace MicroService
{
    ///<summary>
    ///
    ///</summary>
    [XmlType("Country")]
    public class Country
    {
        ///<summary>
        ///
        ///</summary>
        public Guid? country_id{get;set;}        
        ///<summary>
        ///
        ///</summary>
        [Required]
        public int? flag_code{get;set;}
        ///<summary>
        ///
        ///</summary>
        [Required]
        public string name{get;set;}

        ///<summary>
        ///
        ///</summary>
        public Country()
        {

        }

        ///<summary>
        ///
        ///</summary>
        public Country(System.Data.IDataReader r)
        {
            country_id = (Guid)r["country_id"];
            flag_code = (int?)r["flag_code"];
            name = (string)r["name"];    
       }    
 
        ///<summary>
        ///
        ///</summary>
        public override string ToString()
        {
            return String.Format("country_id={0}, flag_code={2}, name={3}", country_id, flag_code, name);
        }        
    }
}