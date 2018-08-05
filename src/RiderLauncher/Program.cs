using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RiderLauncher
{
    class Program
    {
        static readonly Regex VersionNumberExtractor = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+");
        static void Main(string[] args)
        {
            var folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"JetBrains\Toolbox\apps\Rider"
            );
            
            if (Directory.Exists(folderPath))
            {
                var latest = Directory.EnumerateFiles(folderPath, "rider64.exe", SearchOption.AllDirectories)
                    .OrderByDescending(f => VersionNumberExtractor.Match(f).Value)
                    .FirstOrDefault();

                if (latest != null)
                {
                    var arguments = string.Join(" ", args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a));
                    Console.WriteLine($"Starting: {latest} {arguments}");
                    var psi = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = latest,
                        Arguments = arguments
                    };
                    Process.Start(psi);
                }
            }
        }
    }
}
