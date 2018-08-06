using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace RiderLauncher
{
    class Program
    {
        private const string IfThisMakesYouSad = "If this makes you sad, please open an issue at https://github.com/RendleLabs/dotnet-rider-cli";

        static void Main(string[] args)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Console.Error.WriteLine($"This tool only works on Windows. {IfThisMakesYouSad}");
            }
            
            if (ToolboxRiderFinder.TryGetLatestActiveExecutable(out var activeExecutable))
            {
                StartRider(activeExecutable, args);
            }
            else
            {
                Console.WriteLine($"Couldn't find Rider install location. {IfThisMakesYouSad}");
            }
        }

        private static void StartRider(string riderExe, string[] args)
        {
            var arguments = string.Join(" ", args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a));
            
            Console.WriteLine($"Starting: {riderExe} {arguments}");
            
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = riderExe,
                Arguments = arguments
            };
            
            Process.Start(psi);
        }
    }
}