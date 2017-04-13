using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace MicroService
{

    ///<summary>
    ///
    ///</summary>
    [XmlType("VesselQuery")]
    public class VesselQuery
    {     
        ///<summary>
        ///
        ///</summary>
        public int? mmsi{get;set;}
        ///<summary>
        ///
        ///</summary>
        public int? imo{get;set;}
        ///<summary>
        ///
        ///</summary>
        public string vessel_name{get;set;}
        ///<summary>
        ///
        ///</summary>
        public string callsign{get;set;} 
        ///<summary>
        ///
        ///</summary>             
        public int? flag_code{get;set;}
        ///<summary>
        ///
        ///</summary>
        public int? vessel_type_code{get;set;}
        ///<summary>
        ///
        ///</summary>
        public bool IsEpmty()
        {
            return !imo.HasValue &&
                !mmsi.HasValue &&
                (String.IsNullOrEmpty(vessel_name) || vessel_name.Length<2) &&
                (String.IsNullOrEmpty(callsign) || callsign.Length<2) &&
                !flag_code.HasValue &&
                !vessel_type_code.HasValue;
        }
    }
    

 
    ///<summary>
    ///
    ///</summary>   
    public class Database : DbContext
    {
        ///<summary>
        ///
        ///</summary>
        public DbSet<Vessel> Vessel { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Database(DbContextOptions<Database> options)
        : base(options)
        { }


        ///<summary>
        ///
        ///</summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vessel>()
                .HasKey(p => p.vessel_id);
        }
    } 


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
        ///
        ///</summary>
        [Required]
        public int? vessel_type_code{get;set;}

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

        ///<summary>
        ///
        ///</summary>
        public override string ToString()
        {
            return String.Format("vessel_id={0}, mmsi={1}, imo={2}, vessel_name={3}", vessel_id, mmsi, imo, vessel_name);
        }        
    }
}