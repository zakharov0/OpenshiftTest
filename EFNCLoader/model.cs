using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EFNCLoader
{
    public class VesselTrafficContext : DbContext
    {
        public DbSet<Position> Positions { get; set; }
        public DbSet<Position2> Positions2 { get; set; }        
        public DbSet<IceBreaker> IceBreakers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=192.168.14.43;Failover Partner=192.168.14.44;Initial Catalog=Maps;User Id=Maps1410;Password=8ewREh4z");
            //optionsBuilder.UseSqlServer("Data Source=KOSMO-2-PC; Initial Catalog=LayersDB; Integrated Security=true");
         
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("FleetMonAIS");
            modelBuilder.Entity<Position>().HasKey(k => k.PositionId);
            modelBuilder.Entity<Position2>().HasKey(k => k.PositionId);
            modelBuilder.Entity<IceBreaker>().HasKey(k => k.PositionId);
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

    
    public class Position2{

        public long PositionId{ get; set; }
 // Voyage
        public double? DRAUGHT{ get; set; }// 4.4,
        public string DEST{ get; set; }//"MURMANSK",
        public string ETA{ get; set; }//"2017-03-17T02:00:00Z"
        public string NAME{ get; set; }// "M 0345 BUGSY",
 
        public int? MMSI{ get; set; }// 273317970,        
        public string CALLSIGN{ get; set; }// "UFPA",
        public int? TYPE{ get; set; }// "SPC",
        public int? IMO{ get; set; }// 8723725,


    //Position
        public DateTime TIME { get; set; }// "2017-03-21T12:09:51Z",
        public double? COG { get; set; }// 233,
        public int? NAVSTAT { get; set; }// "under way using engine",
        public int? HEADING { get; set; }// 197,
        public double? LONGITUDE { get; set; }// 33.009733,
        public double? LATITUDE { get; set; }// 68.938667,
        public double? SOG { get; set; }// 0

        
        public int? A { get; set; }// 197,
        public int? B { get; set; }// 197,
        public int? C { get; set; }// 197,
        public int? D { get; set; }// 197,


        public Position2()
        {

        }
        public Position2(DataTransfer.Vessel2 ves)
        {
            CALLSIGN = ves.CALLSIGN;
            COG = ves.COG;  
            DEST = ves.DEST;
            DRAUGHT = ves.DRAUGHT;
            ETA = ves.ETA;
            HEADING = ves.HEADING;
            IMO = ves.IMO;
            LATITUDE = ves.LATITUDE;
            LONGITUDE = ves.LONGITUDE;
            MMSI = ves.MMSI;
            NAME = ves.NAME;
            NAVSTAT = ves.NAVSTAT;
            TIME = ves.TIME;
            SOG = ves.SOG;
            TYPE = ves.TYPE;
            A = ves.A;
            B = ves.B;
            C = ves.C;
            D = ves.D;
        }
    }
}