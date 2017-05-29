using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace EFNCLoader
{

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

        public static List<string> GetOptions(string s, string target)
        {            
            var start = -1;
            var end = s.IndexOf(target);//
            while(end!=-1){
                start = s.IndexOf("value=", end);
                if (start<0)
                    break;
                end = s.IndexOf(">", start);
                Console.Write(s.Substring(start, end-start).Replace("selected", "").Replace("value=", "").Trim('"', ' ', '\'')+" ");
                start = end+1;
                end = s.IndexOf("<option", start);
                Console.WriteLine("'{0}'", s.Substring(start, s.IndexOf("<", start)-start).Trim());
                if (start<0 || s.Substring(start, end-start).Contains("select"))
                    break;
            }
            return null;
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

        static void ParseInfo(string s)
        { 
            string d;
            var start = Parse(s, "<div", 0, out d);
            Console.WriteLine(d);
            var group = "";
            while(start!=-1 && (start = Parse(s, "<div", start, out group))!=-1){
                //if (group=="Companies related to the vessel")
                //d="";
                Console.WriteLine("{'group': '"+group+"'}");          
                while((start = Parse(s, "<td", start, out d))!=-1){
                    Console.Write("{'title': '"+d+"', ");
                    start = Parse(s, "<td", start, out d);
                    Console.WriteLine("'value': '"+Regex.Replace(Regex.Replace(d, @"[\r\n]+", ""), @"[\s]+", " ")+"'}");
                    if (s.IndexOf("<td", start)>s.IndexOf("<div", start))
                        break;
                } 
            } 
        }

        public static async Task<int> GetInfoAsync(string s)
        {   
            int start = -1;
            string pattern = "vessel?fleet_id=";
            while((start = s.IndexOf(pattern, start+1))!=-1){
                var request = s.Substring(start, s.IndexOf(")", start)-start).Trim('\'','"',' ');
                Console.WriteLine("{0}.\t{1}", (++Counter), request);

                // var info = await LeechAsync("https://lk.rs-class.org/regbook/print",
                // new StringContent(""), new Uri("https://lk.rs-class.org/regbook/"+request));                
                // ParseInfo(info);
                // break;
            }   
            return 0;     
        }

        static int Counter = 0;

        public static void Main(string[] args)
        {
           
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var s = "";
            if (args.Contains("getoptions"))
            {
                s = LeechAsync("https://lk.rs-class.org/regbook/regbookVessel?ln=ru",
                new StringContent("")).Result;
                GetOptions(s, "stran_id1");//statgr_id
                return;
            }  
            
            string[] countries = {"11"};
            if (args.Contains("getinfo"))
            {
                countries= new string[]{args[1]};
            }

            int page=0;
            s = LeechAsync("https://lk.rs-class.org/regbook/regbookVessel?ln=ru",
            new StringContent(string.Format("stran_id1={0}&pageNav={1}", countries[0], page))).Result;
            Match m = Regex.Match(s, @"(суда|vessels)[^\(]*\((\d+)\)", RegexOptions.IgnoreCase); 
            if(!m.Success)
                throw new Exception("!!!");
            Console.WriteLine("VESSELS: "+m.Groups[2].Value);
            var r = GetInfoAsync(s).Result;

            {
                s = LeechAsync("https://lk.rs-class.org/regbook/regbookVessel?ln=ru",
                new FormUrlEncodedContent(new Dictionary<string,string>(){{"stran_id1", countries[0]}, {"pageNav", (page++).ToString()}})).Result;
                r = GetInfoAsync(s).Result;
            }
            Console.WriteLine("DONE");
        }
    }
}
