using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RiderLauncher
{
    internal static class ToolboxRiderFinder
    {
        static readonly Version MaxVersion = new Version(32767, 32767, 32767);
        static readonly Regex VersionNumberExtractor = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+");

        public static bool TryGetLatestActiveExecutable(out string path)
        {
            path = GetActiveExecutables()
                .OrderByDescending(f => Version.TryParse(VersionNumberExtractor.Match(f).Value, out var version) ? version : MaxVersion)
                .FirstOrDefault();
            return path != null;
        }

        public static IEnumerable<string> GetActiveExecutables()
        {
            var appLocation = GetAppLocation();

            foreach (var channelSettingsFile in Directory.EnumerateFiles(appLocation, ".channel.settings.json", SearchOption.AllDirectories))
            {
                JObject settings;
                using (var reader = File.OpenText(channelSettingsFile))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        settings = JObject.Load(jsonReader);
                    }
                }

                var channelDirectory = Path.GetDirectoryName(channelSettingsFile);
                if (settings["active-application"] is JObject activeApp && activeApp["builds"] is JArray buildArray)
                {
                    foreach (string build in buildArray.Values<string>())
                    {
                        var riderPath = Path.Combine(channelDirectory, build, "bin", "rider64.exe");
                        if (File.Exists(riderPath))
                        {
                            yield return riderPath;
                        }
                    }
                }
            }
        }

        private static string GetAppLocation()
        {
            var toolboxPath = GetToolboxPath();

            var settingsPath = Path.Combine(toolboxPath, ".settings.json");
            
            if (File.Exists(settingsPath))
            {
                JObject settings;
                using (var reader = File.OpenText(settingsPath))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        settings = JObject.Load(jsonReader);
                    }
                }

                if (settings.ContainsKey("install_location"))
                {
                    var toolboxInstallLocation = settings["install_location"].Value<string>();
                    var folderPath = Path.Combine(toolboxInstallLocation, "apps", "Rider");
                    if (Directory.Exists(folderPath))
                    {
                        return folderPath;
                    }
                }
            }

            return Path.Combine(toolboxPath, "apps", "Rider");
        }

        private static string GetToolboxPath()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    return Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share/JetBrains/Toolbox");
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"JetBrains\Toolbox");
                case PlatformID.MacOSX:
                    return Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Library/Application Support/JetBrains/Toolbox");
                default:
                    return null;
            }
        }
    }
}