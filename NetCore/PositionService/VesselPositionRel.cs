using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroService
{ 
    ///<summary>
    ///
    ///</summary>
    [XmlType("VesselRelativePosition")]
    public class VesselRelativePosition
    {   
        ///<summary>
        ///
        ///</summary>
        public long base_position_id{get;set;}        
        ///<summary>
        ///
        ///</summary>
        [Column("state_id")]
        [Key]
        public long position_id{get;set;}       
        ///<summary>
        ///
        ///</summary>
        [Required]
        public Guid vessel_id{get;set;}        
        ///<summary>
        ///
        ///</summary>
        public double? rot{get;set;}        
        ///<summary>
        ///
        ///</summary>
        public double? sog{get;set;}        
        ///<summary>
        ///
        ///</summary>
        public double? cog{get;set;}         
        ///<summary>
        ///
        ///</summary>
        [Required]
        public double longitude{get;set;}         
        ///<summary>
        ///
        ///</summary>
        [Required]
        public double latitude{get;set;}    
        ///<summary>
        ///
        ///</summary>
        public int? heading{get;set;}          
        ///<summary>
        ///
        ///</summary>
        [Required]
        public DateTime ts_pos_utc{get;set;}         
        ///<summary>
        ///
        ///</summary>
        public String vessel_type_cargo{get;set;}         
        ///<summary>
        ///
        ///</summary>
        public int nav_status_code { get; set; }          
        ///<summary>
        ///
        ///</summary>
        [ForeignKeyAttribute("nav_status_code")]
        public VesselNavStatus nav_status { get; set; }
        ///<summary>
        ///
        ///</summary>
        public double? bearing{get;set;}         
        ///<summary>
        ///
        ///</summary>
        public double? distance{get;set;}     

        ///<summary>
        ///
        ///</summary>
        public VesselRelativePosition()
        {

        }

        ///<summary>
        ///
        ///</summary>
        public override string ToString()
        {
            return String.Format("position_id={0}, vessel_id={1}, lon={2}, lat={3}, {4}", position_id , vessel_id, longitude, latitude, ts_pos_utc);
        }        
    }
}