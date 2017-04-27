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
    public class Database : DbContext
    {
        ///<summary>
        ///
        ///</summary>
        public DbSet<VesselPosition> VesselPositions { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Database(DbContextOptions<Database> options)
        : base(options)
        { 
        }


        ///<summary>
        ///
        ///</summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
/*            modelBuilder.Entity<VesselNavStatus>()
                .HasKey(p => p.nav_status_code);

            modelBuilder.Entity<VesselPosition>()
                .HasKey(p => p.position_id);
                
            modelBuilder.Entity<VesselPosition>()
            .HasOne(p => p.NavStatus).WithMany()
            .HasForeignKey(p => p.nav_status_code);
*/

        }
    } 

    ///<summary>
    ///
    ///</summary>
    public class VesselNavStatus
    { 
        ///<summary>
        ///
        ///</summary>
        [Required]
        [Key]
        public int nav_status_code{get;set;}         
        ///<summary>
        ///
        ///</summary>
        public String nav_status{get;set;}         
    }


    ///<summary>
    ///
    ///</summary>
    [Table("VesselState")]
    [XmlType("VesselPosition")]
    public class VesselPosition
    {     
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
        public double? bearing{get;set;}         
        ///<summary>
        ///
        ///</summary>
        public double? distance{get;set;}     

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
        public VesselNavStatus NavStatus { get; set; }
/*
state_id bigint NOT NULL DEFAULT nextval('"VesselState_state_id_seq"'::regclass),
  vessel_id uuid NOT NULL,

  cog real,
  heading integer,
  rot real,
  sog real,
  wkb_geometry geometry,
  longitude numeric,
  latitude numeric,
  ts_pos_utc timestamp without time zone,
  vessel_type_cargo text,
  nav_status_code integer,

  draught numeric,
  eta text,
  port_id uuid,
  ts_pos_utc timestamp without time zone,
  prev_id bigint,
*/ 


        ///<summary>
        ///
        ///</summary>
        public VesselPosition()
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