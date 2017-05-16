using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

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
        public DbSet<VesselType> VesselType { get; set; }

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
            modelBuilder.Entity<VesselType>()
                .HasKey(p => p.vessel_type_code);
        }
    } 
}