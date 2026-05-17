using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Subnautica2BiomодUnlocker
{
    class Program
    {
        static readonly string AppName = "Subnautica 2 — All Biomods Unlocker";
        static readonly string Version = "1.0.0";

        static readonly string[] SteamPaths = new[]
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\Subnautica2",
            @"C:\Program Files\Steam\steamapps\common\Subnautica2",
            @"D:\Steam\steamapps\common\Subnautica2",
            @"E:\Steam\steamapps\common\Subnautica2",
        };

        static readonly string[] EpicPaths = new[]
        {
            @"C:\Program Files\Epic Games\Subnautica2",
            @"D:\Epic Games\Subnautica2",
        };

        static readonly string ConfigRelativePath = Path.Combine("Subnautica2_Data", "StreamingAssets", "configs", "biomods.json");
        static readonly string BackupSuffix = ".backup";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            PrintHeader();

            string? gamePath = FindGamePath();

            if (gamePath == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Subnautica 2 installation not found.");
                Console.WriteLine("        Please enter the path manually:");
                Console.ResetColor();
                Console.Write("> ");
                gamePath = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Invalid path. Exiting.");
                    Console.ResetColor();
                    Exit(1);
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK]    Game found at: {gamePath}");
            Console.ResetColor();

            string configPath = Path.Combine(gamePath, ConfigRelativePath);

            if (!File.Exists(configPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Config file not found: {ConfigRelativePath}");
                Console.ResetColor();
                Exit(1);
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Choose an action:");
            Console.WriteLine("  [1] Unlock All Biomods");
            Console.WriteLine("  [2] Restore Original Files");
            Console.WriteLine("  [0] Exit");
            Console.WriteLine();
            Console.Write("> ");

            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    UnlockAllBiomods(configPath);
                    break;
                case "2":
                    RestoreBackup(configPath);
                    break;
                case "0":
                    Console.WriteLine("Goodbye!");
                    break;
                default:
                    Console.WriteLine("[INFO] Unknown option. Exiting.");
                    break;
            }

            Exit(0);
        }

        static void UnlockAllBiomods(string configPath)
        {
            Console.WriteLine();
            Console.WriteLine("[INFO] Creating backup...");

            string backupPath = configPath + BackupSuffix;

            if (!File.Exists(backupPath))
            {
                File.Copy(configPath, backupPath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[OK]   Backup saved to: {backupPath}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("[INFO] Backup already exists. Skipping.");
            }

            Console.WriteLine("[INFO] Reading config...");

            string json = File.ReadAllText(configPath);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            var unlockedConfig = BuildUnlockedConfig(root);

            string outputJson = JsonSerializer.Serialize(unlockedConfig, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(configPath, outputJson);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║   ✓  All Biomods Unlocked!           ║");
            Console.WriteLine("║   Launch Subnautica 2 and enjoy.     ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.ResetColor();
        }

        static Dictionary<string, object> BuildUnlockedConfig(JsonElement original)
        {
            var config = new Dictionary<string, object>();

            // Preserve all original fields
            foreach (JsonProperty prop in original.EnumerateObject())
            {
                if (prop.Name == "biomods")
                {
                    // Unlock all biomods
                    var biomods = new Dictionary<string, object>();
                    foreach (JsonProperty mod in prop.Value.EnumerateObject())
                    {
                        biomods[mod.Name] = new Dictionary<string, object>
                        {
                            ["unlocked"] = true,
                            ["level"] = GetMaxLevel(mod),
                            ["enabled"] = true
                        };
                    }
                    config["biomods"] = biomods;
                }
                else
                {
                    config[prop.Name] = prop.Value.GetRawText();
                }
            }

            // Add known biomod categories if not present
            if (!config.ContainsKey("biomods"))
            {
                config["biomods"] = BuildDefaultBiomods();
            }

            return config;
        }

        static int GetMaxLevel(JsonProperty mod)
        {
            if (mod.Value.TryGetProperty("maxLevel", out JsonElement maxLevelEl))
                return maxLevelEl.GetInt32();
            return 3; // default max level
        }

        static Dictionary<string, object> BuildDefaultBiomods()
        {
            var categories = new[]
            {
                "cephalopod_camouflage",
                "bioluminescence_enhancement",
                "predator_resistance",
                "flora_absorption",
                "mutagenic_adaptation",
                "neural_sync",
                "respiratory_mod",
                "bioelectric_mod",
                "exoskeletal_mod",
                "symbiotic_mod",
                "thermal_resistance",
                "pressure_adaptation",
                "acid_immunity",
                "echo_location",
                "regenerative_tissue"
            };

            var biomods = new Dictionary<string, object>();
            foreach (var id in categories)
            {
                biomods[id] = new Dictionary<string, object>
                {
                    ["unlocked"] = true,
                    ["level"] = 3,
                    ["enabled"] = true
                };
            }
            return biomods;
        }

        static void RestoreBackup(string configPath)
        {
            string backupPath = configPath + BackupSuffix;

            if (!File.Exists(backupPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[WARN] No backup found. Nothing to restore.");
                Console.ResetColor();
                return;
            }

            File.Copy(backupPath, configPath, overwrite: true);
            File.Delete(backupPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[OK]   Original files restored successfully.");
            Console.ResetColor();
        }

        static string? FindGamePath()
        {
            Console.WriteLine("[INFO] Searching for Subnautica 2 installation...");

            foreach (var path in SteamPaths)
            {
                if (Directory.Exists(path))
                    return path;
            }

            foreach (var path in EpicPaths)
            {
                if (Directory.Exists(path))
                    return path;
            }

            return null;
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine($"║   🧬  {AppName,-38}  ║");
            Console.WriteLine($"║       Version {Version,-35}  ║");
            Console.WriteLine("║       github.com/YOUR_USERNAME/sn2-biomods      ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void Exit(int code)
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(code);
        }
    }
}
