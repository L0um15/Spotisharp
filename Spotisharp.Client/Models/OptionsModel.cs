using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotisharp.Client.Models
{
    public class OptionsModel
    {

        [Option('u', "check-updates", Required = false, HelpText = "Default: True, if false skips update check.")]
        public bool? CheckUpdates { get; set; }

        [Option("keep-options", Required = false, HelpText = "If set to true, saves all launch parameters to config file")]
        public bool? KeepOptions { get; set; }

        [Option('w', "workers", Required = false, HelpText = "Set amount of workers needed for simultaneous downloading")]
        public int? WorkersCount { get; set; }

        [Option('i', "input", Required = false, HelpText = "Type title of the song, url of the song or playlist or album")]
        public string? UserInput { get; set; }

        [Option('o', "output-dir", Required = false, HelpText = "Specify output directory for spotisharp to handle.")]
        public string? OutputDir { get; set; }
    }
}
