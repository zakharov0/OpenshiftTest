using System;
using Newtonsoft.Json;

namespace DataTransfer
{

    public class Voyage{
        public double? draught{ get; set; }// 4.4,
        public DateTime received{ get; set; }//2017-03-21T12:07:48Z",
        public string destination{ get; set; }//"MURMANSK",
        public string special_cargo{ get; set; }// null,
        public DateTime? eta{ get; set; }//"2017-03-17T02:00:00Z"
    
    }


    public class Vessel2
    {
        public int? MMSI{ get; set; }// 273317970,        
        public DateTime TIME{ get; set; }//2017-03-21T12:07:48Z",
        public double? LONGITUDE{get;set;}
        public double? LATITUDE{get;set;}
        public double? COG { get; set; }// 233,
        public double? SOG { get; set; }// 0
        public int? HEADING { get; set; }// 197,
        public int? NAVSTAT { get; set; }// "under way using engine",
        public int? IMO{ get; set; }// 8723725,
        public string NAME{ get; set; }// "M 0345 BUGSY",        
        public string CALLSIGN{ get; set; }// "UFPA",
        public int? TYPE{ get; set; }// "[not available]",
        public int? A{ get; set; }// "[not available]",
        public int? B{ get; set; }// "[not available]",
        public int? C{ get; set; }// "[not available]",
        public int? D{ get; set; }// "[not available]",
        public double? DRAUGHT{ get; set; }// 129,
        public string DEST{ get; set; }//"MURMANSK",
        public string ETA{ get; set; }//"2017-03-17T02:00:00Z"

        public override string ToString()
        {
            return String.Format("{0}\t{1}\t{2}", MMSI, IMO, NAME);
        }

    }

    public class Vessel
    {  
        public string name{ get; set; }// "M 0345 BUGSY",
        public int? dwt{ get; set; }// 129,
        public int? mmsi_number{ get; set; }// 273317970,
        public string ais_type_of_ship_str{ get; set; }// "[not available]",
        public string type_class{ get; set; }// "unknown",
        public string cn_iso2{ get; set; }// "RU",
        public int? width{ get; set; }// 8,
        public int? length{ get; set; }// 36,
        public string callsign{ get; set; }// "UFPA",
        public string type_code{ get; set; }// "SPC",
        public int? imo_number{ get; set; }// 8723725,
        public string country{ get; set; }// "Russia",
        public string type{ get; set; }// "Special Purpose",
        public int? ais_type_of_ship{ get; set; }// 0,
        public long vessel_id{ get; set; }// 8753740

        public Voyage voyage{get;set;}
        public Position position{get;set;}

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
    public class Position
    {
        public DateTime received { get; set; }// "2017-03-21T12:09:51Z",
        public double? course_over_ground { get; set; }// 233,
        public string nav_status { get; set; }// "under way using engine",
        public int? true_heading { get; set; }// 197,
        public double? longitude { get; set; }// 33.009733,
        public double? latitude { get; set; }// 68.938667,
        public string source { get; set; }// "terrestrial",
        public bool in_special_maneuver { get; set; }// false,
        public double? speed { get; set; }// 0

    }
}