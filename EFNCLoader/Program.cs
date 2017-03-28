using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.IO;

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

        public static void Main(string[] args)
        {
            while(true)
            {
                var vessels_received = (DataTransfer.Vessel[])Run((obj)=>{
                        //throw new Exception("TEST");
                        string stringData;
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = (Uri)obj;
                            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            HttpResponseMessage response = client.GetAsync("/regional_ais/?apikey=2add5120c9e5f4a4a33c8a5d75167880").Result;
                            stringData = response.Content.ReadAsStringAsync().Result;
                        }
                                    
                        string vessArray = stringData
                            .Remove(stringData.LastIndexOf("}"))
                            .Substring(stringData.IndexOf("["));
                        return JsonConvert.DeserializeObject<DataTransfer.Vessel[]>(vessArray);
                }, new Uri("https://apiv2.fleetmon.com"));
                
                Run((obj)=>{
                    using (var db = new VesselTrafficContext())
                    {
                        var vr = (DataTransfer.Vessel[]) obj;
                        foreach (var vessel in vr)
                        {
                            try
                            {
                                //throw new Exception("TEST DB FAIL");
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
                            }
                            catch (Exception e)
                            { throw new Exception(e + "\n" + vessel); }
                        }
                    }
                    return null;
                }, vessels_received);

                Console.WriteLine("==============");
                Console.WriteLine(DateTime.Now);
                Console.WriteLine("==============");
                System.Threading.Thread.Sleep(300*1000);
            }
        }
    }
}
