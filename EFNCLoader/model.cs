using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EFNCLoader
{
    public class VesselTrafficContext : DbContext
    {
        public DbSet<Position> Positions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=192.168.14.43;Failover Partner=192.168.14.44;Initial Catalog=Maps;");
            //optionsBuilder.UseSqlServer("Data Source=KOSMO-2-PC; Initial Catalog=LayersDB; Integrated Security=true");
         
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("FleetMonAIS");
        }
    }

    public class Position{

        public long PositionId{ get; set; }
 // Voyage
        public double? Draught{ get; set; }// 4.4,
        public DateTime VoyageReceived{ get; set; }//2017-03-21T12:07:48Z",
        public string Destination{ get; set; }//"MURMANSK",
        public string Special_cargo{ get; set; }// null,
        public DateTime? Eta{ get; set; }//"2017-03-17T02:00:00Z"
    
    // Vessel  
        public string Name{ get; set; }// "M 0345 BUGSY",
        public int? Dwt{ get; set; }// 129,
        public int? Mmsi{ get; set; }// 273317970,
        public string Ais_type_of_ship_str{ get; set; }// "[not available]",
        public string Type_class{ get; set; }// "unknown",
        public string Cn_iso2{ get; set; }// "RU",
        public int? Width{ get; set; }// 8,
        public int? Length{ get; set; }// 36,
        public string Callsign{ get; set; }// "UFPA",
        public string Type_code{ get; set; }// "SPC",
        public int? Imo{ get; set; }// 8723725,
        public string Country{ get; set; }// "Russia",
        public string Type{ get; set; }// "Special Purpose",
        public int? Ais_type_of_ship{ get; set; }// 0,
        public long VesselId{ get; set; }// 8753740


    //Position
        public DateTime PositionReceived { get; set; }// "2017-03-21T12:09:51Z",
        public double? Cog { get; set; }// 233,
        public string Nav_status { get; set; }// "under way using engine",
        public int? True_heading { get; set; }// 197,
        public double? Longitude { get; set; }// 33.009733,
        public double? Latitude { get; set; }// 68.938667,
        public string Source { get; set; }// "terrestrial",
        public bool In_special_maneuver { get; set; }// false,
        public double? Sog { get; set; }// 0


        public Position()
        {

        }
        public Position(DataTransfer.Vessel ves)
        {
            this.Draught = ves.voyage.draught;
            this.VoyageReceived = ves.voyage.received;
            this.Destination = ves.voyage.destination;
            this.Special_cargo = ves.voyage.special_cargo;
            this.Eta = ves.voyage.eta;
    
            this.Name = ves.name;
            this.Dwt = ves.dwt;
            this.Mmsi = ves.mmsi_number;
            this.Ais_type_of_ship_str = ves.ais_type_of_ship_str;
            this.Type_class = ves.type_class;
            this.Cn_iso2 = ves.cn_iso2;
            this.Width = ves.width;
            this.Length = ves.length;
            this.Callsign = ves.callsign;
            this.Type_code = ves.type_code;
            this.Imo = ves.imo_number; 
            this.Country = ves.country;
            this.Type = ves.type;
            this.Ais_type_of_ship = ves.ais_type_of_ship;
            this.VesselId = ves.vessel_id;

            this.PositionReceived = ves.position.received;
            this.Cog = ves.position.course_over_ground;
            this.Nav_status = ves.position.nav_status;
            this.True_heading = ves.position.true_heading;
            this.Longitude = ves.position.longitude;
            this.Source = ves.position.source;
            this.In_special_maneuver = ves.position.in_special_maneuver;
            this.Latitude = ves.position.latitude;
            this.Sog = ves.position.speed;

        }
    }
}