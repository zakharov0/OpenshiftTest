using Microsoft.EntityFrameworkCore;
using System;
//using System.Reflection;
//using System.Linq;
using System.Xml.Serialization;

namespace VesselService
{
    public class Database : DbContext
    {
        public DbSet<VesselInfo> VesselInfo { get; set; }

        public Database(DbContextOptions<Database> options)
        : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder.Entity<VesselInfo>()
                .HasKey(p => p.guid);
        }
    } 

    [XmlType("Vessel")]
    public class VesselInfo
    {
        public string guid{get;set;}
        public int? mmsi{get;set;}
        public int? imo{get;set;}
        public string vessel_name{get;set;}
        public string callsign{get;set;}                    
        public string flag_country{get;set;}
        public string vessel_type{get;set;}

        //public int? width{get;set;}
        //public int? height{get;set;}

        public VesselInfo()
        {

        }
        public VesselInfo(System.Data.IDataReader r)
        {
            guid = (r["guid"].ToString());
            mmsi = (int?)r["mmsi"];
            imo = (int?)r["imo"];
            vessel_name = (string)r["vessel_name"];
            callsign = (string)r["callsign"];              
            flag_country = (string)r["flag_country"];
            vessel_type = (string)r["vessel_type"];      
        }           
    }
}