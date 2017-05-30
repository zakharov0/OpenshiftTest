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
        public DbSet<VesselRelativePosition> VesselRelativePositions { get; set; }

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
        }
    } 

}