using System;
using System.IO;

namespace Subnautica2BiomодUnlocker
{
    public static class BackupManager
    {
        private const string BackupSuffix = ".original.backup";

        public static bool CreateBackup(string filePath)
        {
            string backupPath = filePath + BackupSuffix;

            if (File.Exists(backupPath))
            {
                Console.WriteLine("[INFO] Backup already exists, skipping creation.");
                return true;
            }

            try
            {
                File.Copy(filePath, backupPath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[OK]   Backup created: {Path.GetFileName(backupPath)}");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Failed to create backup: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public static bool RestoreBackup(string filePath)
        {
            string backupPath = filePath + BackupSuffix;

            if (!File.Exists(backupPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[WARN] No backup found. Nothing to restore.");
                Console.ResetColor();
                return false;
            }

            try
            {
                File.Copy(backupPath, filePath, overwrite: true);
                File.Delete(backupPath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[OK]   Files restored to original state.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Restore failed: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public static bool HasBackup(string filePath)
        {
            return File.Exists(filePath + BackupSuffix);
        }
    }
}
