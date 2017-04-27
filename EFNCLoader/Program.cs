using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace EFNCLoader
{

    public class Program
    {
        static object Run(Func<object, object> f, object arg1){
            int EFFORTS = 3;
            int count = 0;

            while (true)
            {
                try
                {
                    if (arg1==null)
                        return null;
                    return f(arg1);
                }
                catch (Exception e)
                {
                    if (++count > EFFORTS)
                    {
                        using (var tw = new StreamWriter(File.Open("error.log", FileMode.Append)))
                        {
                            tw.WriteLine(DateTime.Now);
                            tw.WriteLine(e);
                            tw.WriteLine("=============================================");
                        }
                        return null;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        enum Provider
        {
            FleetMon=0,
            VtExplorer=1,
            FleetMonFleet
        }

        public static void Main(string[] args)
        {
            Provider provider = Provider.FleetMonFleet;
            string[] uri = {
                "https://apiv2.fleetmon.com",
                "https://ws.vtexplorer.com",
                "https://apiv2.fleetmon.com"
            };
            string[] path = {  
                "/regional_ais/?apikey=2add5120c9e5f4a4a33c8a5d75167880",
                "/?username=SCANEX&password=5c9Ln34G2&method=getSnapshot&format=1&output=json&compress=0",
                "/fleet/?apikey=403e256847ec1282bfe9ece271608a82"
            }; 
            while(true)
            {
                var vessels_received = Run((obj)=>{
                        //throw new Exception("TEST");
                        string stringData;
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = (Uri)obj;
                            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            HttpResponseMessage response = client.GetAsync(path[(int)provider]).Result;
                            stringData = response.Content.ReadAsStringAsync().Result;
                        }

                        switch (provider)
                        {   
                            case Provider.FleetMon:  
                            {       
                                string vessArray = stringData
                                    .Remove(stringData.LastIndexOf("}"))
                                    .Substring(stringData.IndexOf("[")); 
                                return JsonConvert.DeserializeObject<DataTransfer.Vessel[]>( vessArray);
                            }
                            case Provider.FleetMonFleet:
                            {       
                                string vessArray = stringData
                                    .Remove(stringData.LastIndexOf("}"))
                                    .Substring(stringData.IndexOf("[")); 
                                return JsonConvert.DeserializeObject<DataTransfer.IceBraker[]>( vessArray);
                            }
                            case Provider.VtExplorer:
                            {
                                string vessArray = stringData
                                    .Remove(stringData.LastIndexOf("]"))
                                    .Substring(stringData.IndexOf("[", 2));
                                return JsonConvert.DeserializeObject<DataTransfer.Vessel2[]>(
                                //    Regex.Replace(vessArray, @".TIME.:([^,]+)\s*(,|})",  "\"TIME\":new Date(${1}000)$2")
                                //, new JavaScriptDateTimeConverter());
                                vessArray);
                            }
                            default:
                            return null; 
                        }
                }, 
                new Uri(uri[(int) provider]));
                
                Run((obj)=>{
                    using (var db = new VesselTrafficContext())
                    {

                        var vr = (object[]) obj;
                        foreach (var vesobj in vr)
                        {
                            try
                            {
                                //throw new Exception("TEST DB FAIL");
                                switch (provider)
                                {
                                    case Provider.FleetMon:
                                    {
                                        var vessel = (DataTransfer.Vessel)vesobj;
                                        var p = (from pos in db.Positions
                                                where pos.PositionReceived == vessel.position.received &&
                                                pos.VesselId == vessel.vessel_id && pos.Source == vessel.position.source
                                                select pos).SingleOrDefault();                                       
                                        if (p == null)
                                        {
                                            Console.WriteLine(vessel);
                                            db.Positions.Add(new Position(vessel));
                                            db.SaveChanges();
                                        }
                                        break;
                                    }
                                    case Provider.FleetMonFleet:
                                    {
                                        var vessel = (DataTransfer.IceBraker)vesobj;
                                        var p = (from pos in db.IceBreakers
                                                where pos.PositionReceived == vessel.position.received &&
                                                pos.VesselId == vessel.vessel_id && pos.Source == vessel.position.source
                                                select pos).SingleOrDefault();                                       
                                        if (p == null)
                                        {
                                             Console.WriteLine(vessel);
                                            db.IceBreakers.Add(new IceBreaker(vessel));
                                            db.SaveChanges();
                                        }
                                        break;
                                    }
                                    case Provider.VtExplorer:
                                    {
                                        var vessel = (DataTransfer.Vessel2)vesobj;
                                        var p = (from pos in db.Positions2
                                                where pos.TIME == vessel.TIME &&
                                                pos.MMSI == vessel.MMSI && 
                                                pos.LONGITUDE == vessel.LONGITUDE && pos.LATITUDE == vessel.LATITUDE
                                                select pos).FirstOrDefault();                                        
                                        if (p == null)
                                        {
                                            Console.WriteLine(vessel);
                                            db.Positions2.Add(new Position2(vessel));
                                            db.SaveChanges();
                                        }
                                        break;
                                    }
                                }
                            }
                            catch (Exception e)
                            { throw new Exception(e + "\n" + vesobj); }
                        }
                    }
                    return null;
                }, vessels_received);

                Console.WriteLine("==============");
                Console.WriteLine(DateTime.Now);
                Console.WriteLine("==============");
                System.Threading.Thread.Sleep(600*1000);
            }
        }
    }
}
