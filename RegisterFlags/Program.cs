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

namespace Leechit
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
            if (end!=-1)
                throw new ArgumentException(target + " not found");

            using(var textFile = File.CreateText("Flags.txt"))
            while(end!=-1){
                start = s.IndexOf("value=", end);
                if (start<0)
                    break;
                end = s.IndexOf(">", start);

                string line = s.Substring(start, end-start).Replace("selected", "").Replace("value=", "").Trim('"', ' ', '\'')+" ";
                start = end+1;
                end = s.IndexOf("<option", start);
                line += "'"+s.Substring(start, s.IndexOf("<", start)-start).Trim()+"'";
                Console.WriteLine(line);
                textFile.WriteLine(line);
                if (start<0 || s.Substring(start, end-start).Contains("select"))
                    break;
            }
            return null;
        }

        public static void Main(string[] args)
        {
           
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var s = LeechAsync("https://lk.rs-class.org/regbook/regbookVessel?ln=ru",
                new StringContent("")).Result;
                GetOptions(s, "stran_id1");//statgr_id
            return;
        }
    }
}
