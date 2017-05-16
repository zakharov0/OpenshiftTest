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
        string _connStr;

        ///<summary>
        ///
        ///</summary>
        public Database(string connStr)
        { 
            //Console.WriteLine("CTR>>>>>>>>>>>>>" + connStr);
            _connStr = connStr;
        }  

        ///<summary>
        ///
        ///</summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        {
            //Console.WriteLine("ONCGG>>>>>>>>>>>>>" + _connStr);
            if (_connStr!=null)
            optionsBuilder.UseNpgsql(_connStr);
        }


        ///<summary>
        ///
        ///</summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vessel>()
                .HasKey(p => p.vessel_id);
        }
    } 

}