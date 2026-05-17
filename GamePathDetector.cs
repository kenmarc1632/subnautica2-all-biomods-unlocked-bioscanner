using System;
using System.IO;
using Microsoft.Win32;

namespace Subnautica2BiomодUnlocker
{
    public static class GamePathDetector
    {
        private static readonly string[] KnownSteamDrives = { "C", "D", "E", "F", "G" };

        public static string? Detect()
        {
            // 1. Try registry (Steam)
            string? fromRegistry = TryGetSteamPathFromRegistry();
            if (fromRegistry != null) return fromRegistry;

            // 2. Try common paths
            foreach (var drive in KnownSteamDrives)
            {
                var candidates = new[]
                {
                    $@"{drive}:\Program Files (x86)\Steam\steamapps\common\Subnautica2",
                    $@"{drive}:\Program Files\Steam\steamapps\common\Subnautica2",
                    $@"{drive}:\SteamLibrary\steamapps\common\Subnautica2",
                    $@"{drive}:\Steam\steamapps\common\Subnautica2",
                    $@"{drive}:\Program Files\Epic Games\Subnautica2",
                    $@"{drive}:\Epic Games\Subnautica2",
                };

                foreach (var path in candidates)
                {
                    if (Directory.Exists(path))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"[DEBUG] Found at: {path}");
                        Console.ResetColor();
                        return path;
                    }
                }
            }

            return null;
        }

        private static string? TryGetSteamPathFromRegistry()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key == null) return null;

                string? steamPath = key.GetValue("SteamPath") as string;
                if (steamPath == null) return null;

                string gamePath = Path.Combine(steamPath, "steamapps", "common", "Subnautica2");
                return Directory.Exists(gamePath) ? gamePath : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
