using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Text.RegularExpressions;

namespace FSNScan
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://www.flipkart.com/anyproductnamehere/p/itemcode?pid=");
            Console.WriteLine("-------- Reading FSNs -----------");
            List<string> FSNs = new List<string>();
            using (StreamReader sr = new StreamReader("FSN.csv"))
                while (!sr.EndOfStream)
                    FSNs.Add(sr.ReadLine().Split(',')[0]);

            if (FSNs.Count >= 2)
            {
                if(FSNs[0].Length <= 10)
                    FSNs.RemoveAt(0);
                FSNs.Distinct();
            }
            foreach (string fsn in FSNs)
            {
                Console.Write(string.Format("Processing - {0}", fsn));

                try
                {
                    string res = client.GetStringAsync(string.Format("https://www.flipkart.com/anyproductnamehere/p/itemcode?pid={0}", fsn)).Result;
                    if (res != null && res.ToLower().Contains("h1"))
                    {
                        int Start = res.ToLower().IndexOf("<h1");
                        int End = res.ToLower().IndexOf("</h1>");
                        res = res.Substring(Start, End - Start);
                        res = Regex.Replace(res, @"<[^>]*>", string.Empty).Replace(@"&nbsp;", string.Empty);
                        res = Regex.Replace(res, @"[^\u0000-\u007F]+", string.Empty);
                        //FSNs[FSNs.FindIndex(m => m == fsn)] = fsn + ", " + res;
                        Console.Write(" : " + res);
                        using (StreamWriter sw = new StreamWriter("FSN_out.csv", true,System.Text.Encoding.Default))
                            sw.WriteLine(fsn + ", " + SanitizeCSV(res));
                    }
                }
                catch
                {
                    using (StreamWriter sw = new StreamWriter("FSN_out.csv", true, System.Text.Encoding.Default))
                        sw.WriteLine(fsn + ", " + SanitizeCSV("Error"));
                }
                
                Console.WriteLine("");
                System.Threading.Thread.Sleep(1000);
            }

            Console.ReadKey();
        }

        private static string SanitizeCSV(string data)
        {
           
            if (data.Contains(","))
            {
                data = data.Replace(",", " ");
            }
            
            return data;
        }
    }
}