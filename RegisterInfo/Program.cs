using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
using System.IO;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace RegisterInfo
{
    public class Property{
        public int n;
        public string title;        
        public string value;
        public string title_ru;        
        public string value_ru;

    }
    public class PropertyGroup{
        public int n;
        public string title; 
        public string title_ru;  
        public List<Property> properties  = new List<Property>();
    }

    public class VesselCard{
        public List<PropertyGroup> groups  = new List<PropertyGroup>();

    }

    static class ConsoleExt{
        public static string Encoded(this String s){
            Encoding srcEncodingFormat = Encoding.UTF8;
            Encoding dstEncodingFormat = Encoding.GetEncoding("windows-1251");
            return dstEncodingFormat.GetString(Encoding.Convert(srcEncodingFormat, dstEncodingFormat, srcEncodingFormat.GetBytes(s)));
        }
    }
    public class Program
    {
        public static async Task<string> LeechAsync(string url, HttpContent content, Uri referrer=null, string locale="en")
        {
            Uri uri = new Uri("https://lk.rs-class.org");
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            handler.CookieContainer.Add(uri, new Cookie("org.springframework.web.servlet.i18n.CookieLocaleResolver.LOCALE", locale)); // Adding a Cookie
            using (HttpClient client = new HttpClient(handler))
            {
                if (referrer !=null)
                    client.DefaultRequestHeaders.Referrer = referrer;
                var response = await client.PostAsync(url, content);
                var ba =  await response.Content.ReadAsByteArrayAsync();
                var s = Encoding.UTF8.GetString(ba);
                return s;
             }
        }

        static int Parse(string s, string target, int start, out string rv)
        {  
            rv = "";           
            start = s.IndexOf(target, start);
            var end=0;
            if (start==-1)
                return start;
            start = s.IndexOf(">", start)+1;
            end = s.IndexOf("<", start);
            while(s[end+1]=='a' || s[end+2]=='a' || s[end+1]=='b' || s[end+2]=='b')
                end = s.IndexOf("<", end+1);
            rv = s.Substring(start, end-start).Trim();
            return end;
        }

        static void ParseInfo(string s, string fn)
        { 
            string d;
            var start = Parse(s, "<div", 0, out d);
            //Console.WriteLine(d);
            int gc = 0, pc = 0;
            var group = "";
            using(var cardFile = File.CreateText(fn)){
                var card = new VesselCard();
                while(start!=-1 && (start = Parse(s, "<div", start, out group))!=-1){
                    //card.WriteLine("{'group': '"+group+"'}");   
                    var gr = new PropertyGroup(){n=++gc, title=group}; 
                    pc = 0;  
                    while((start = Parse(s, "<td", start, out d))!=-1){
                        var pr = new Property(){n=++pc, title=d};
                        //card.Write("{'title': '"+d+"', ");
                        start = Parse(s, "<td", start, out d);
                        pr.value = d;
                        gr.properties.Add(pr);
                        //card.WriteLine("'value': '"+Regex.Replace(Regex.Replace(d, @"[\r\n]+", ""), @"[\s]+", " ")+"'}");
                        if (s.IndexOf("<td", start)>s.IndexOf("<div", start))
                            break;
                    } 
                    card.groups.Add(gr);
                } 
                cardFile.Write(JsonConvert.SerializeObject(card));
            }
        }

        public static async Task GetInfoAsync(string option, string request, string locale)
        {   
            var info = await LeechAsync("https://lk.rs-class.org/regbook/print",
            new StringContent(""), new Uri("https://lk.rs-class.org/regbook/"+request), locale);                
            ParseInfo(info, option+"\\"+request.Replace("vessel?fleet_id=", "")+"."+locale);
        }

        public static void GetId(string s)
        {   
            int start = -1;
            string pattern = "vessel?fleet_id=";
            while((start = s.IndexOf(pattern, start+1))!=-1){
                var request = s.Substring(start, s.IndexOf(")", start)-start).Trim('\'','"',' ');
                Console.WriteLine("{0}.\t{1}", (++_counter), request);
                _requests.Enqueue(request);
            }      
        }

        static int _counter = 0;

        static ConcurrentQueue<string> _requests = new ConcurrentQueue<string>();

        public static void Main(string[] args)
        {
           
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var s = "";
           
//var info = LeechAsync("https://lk.rs-class.org/regbook/print",
//new StringContent(""), new Uri("https://lk.rs-class.org/regbook/vessel?fleet_id=922050"), "en").Result;
//return;

            string[] countries = {"11"};
            if (args.Contains("getinfo"))
            {
                countries= new string[]{args[1]};
            }

            if (!Directory.Exists(countries[0].ToString()))
            Directory.CreateDirectory(countries[0].ToString());

            var tokenSource = new System.Threading.CancellationTokenSource();
            var cpus = Environment.ProcessorCount;
            Console.WriteLine("CPU "+cpus);
            Task[] tasks = new Task[cpus];
            // for (int i=0; i<tasks.Length-1; ++i)
            //     tasks[i] = Task.Run(()=>{
            //         Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId+" STARTED");
            //         while(true){
            //             string request = "";
            //             if(_requests.TryDequeue(out request))
            //             {
            //                 GetInfoAsync(countries[0], request, "en").Wait();
            //                 GetInfoAsync(countries[0], request, "ru").Wait();
            //                 Console.WriteLine(request + "DONE");
            //             }
            //             else if(tokenSource.Token.IsCancellationRequested)
            //                 break;
            //             else
            //                 Task.Delay(100);
            //         }
            //         Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId+" ENDED");
            //     }, tokenSource.Token);
            tasks[cpus-1] = Task.Run(()=>{
                Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId+" STARTED");
                int page = 0, vessels = 0;
                s = LeechAsync("https://lk.rs-class.org/regbook/regbookVessel?ln=ru",
                new FormUrlEncodedContent(new Dictionary<string, string>(){{"stran_id1", countries[0]}, {"pageNav",  (++page).ToString()}}))
                .Result;
                Match m = Regex.Match(s, @"(суда|vessels)[^\(]*\((\d+)\)", RegexOptions.IgnoreCase); 
                if(!m.Success)
                    throw new Exception("NO VESSELS!!!");
                vessels = int.Parse(m.Groups[2].Value);
                Console.WriteLine("VESSELS: "+vessels);
                GetId(s);
                while(_counter<vessels)
                {
                    s = LeechAsync("https://lk.rs-class.org/regbook/regbookVessel?ln=ru",
                    new FormUrlEncodedContent(new Dictionary<string,string>(){{"stran_id1", countries[0]}, {"pageNav", (++page).ToString()}})).Result;
                    GetId(s);
                }
                Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId+" ENDED");
                tokenSource.Cancel();
            });
            //Task.WaitAll(tasks);
            tasks[cpus-1].Wait();
            
            Console.WriteLine("DONE "+_requests.Count);
        }
    }
}
