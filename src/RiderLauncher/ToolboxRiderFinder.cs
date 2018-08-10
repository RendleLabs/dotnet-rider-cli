using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RiderLauncher
{
    internal static class ToolboxRiderFinder
    {
        private static readonly Version MaxVersion = new Version(32767, 32767, 32767);
        private static readonly Regex VersionNumberExtractor = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+");

        public static bool TryGetLatestActiveExecutable(out string path)
        {
            path = GetActiveExecutables()
                .OrderByDescending(f => Version.TryParse(VersionNumberExtractor.Match(f).Value, out var version) ? version : MaxVersion)
                .FirstOrDefault();
            return path != null;
        }

        private static IEnumerable<string> GetActiveExecutables()
        {
            var appLocation = GetAppLocation();

            if (string.IsNullOrWhiteSpace(appLocation)) yield break;

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
                    if (!string.IsNullOrWhiteSpace(toolboxInstallLocation))
                    {
                        return NullIfNotExists(Path.Combine(toolboxInstallLocation, "apps", "Rider"));
                    }
                }
            }

            return NullIfNotExists(Path.Combine(toolboxPath, "apps", "Rider"));
        }

        private static string NullIfNotExists(string path) => (!string.IsNullOrWhiteSpace(path)) && Directory.Exists(path) ? path : null;

        private static string GetToolboxPath()
        {
            if (OperatingSystem.IsWindows())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"JetBrains\Toolbox");
            if (OperatingSystem.IsLinux())
                return Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share/JetBrains/Toolbox");
            if (OperatingSystem.IsMacOS())
                return Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Library/Application Support/JetBrains/Toolbox");
            return null;
        }
    }

    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}