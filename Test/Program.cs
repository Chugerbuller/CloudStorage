using System.Net;
using System.Net.WebSockets;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var http = new HttpClient();
            var res = http.GetAsync("http://localhost:5000/api/File/LINQ").Result;
            res.Content.ReadAsStringAsync().Wait();
            var web = new WebClient();
            web.DownloadFile("http://localhost:5000/api/File/LINQ", "LINQ.pdf");
            Console.WriteLine(res);
        }
    }
}
