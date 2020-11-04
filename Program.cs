using SpotifyAPI.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary;

namespace SpotiSharp
{
    
    class Program
    {
        static void Main(string[] args) 
            => new SpotiSharp().MainAsync(args).GetAwaiter().GetResult();
    }
}
