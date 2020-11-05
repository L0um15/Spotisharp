using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpotiSharp
{
    class VersionChecker
    {
        private string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        public void checkForUpdates()
        {
            try
            {
                string versionFromUrl = new WebClient().DownloadString("https://raw.githubusercontent.com/L0um15/SpotiSharp/main/version.txt");
                if (versionFromUrl != Version)
                {
                    Console.WriteLine($"SpotiSharp is outdated.\n" +
                        $"Current Version: {Version}\n" +
                        $"Latest Version: {versionFromUrl}");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("SpotiSharp is up to date with the latest version.");
                }
            }
            catch (WebException)
            {
                Console.WriteLine("Something wrong with Github. Unable to check for updates.");
            }
        }


    }
}
