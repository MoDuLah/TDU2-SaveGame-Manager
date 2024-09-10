using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TDU2SaveGameManager.Properties;

namespace TDU2SaveGameManager.Classes
{
    public class SettingsCheck
    {
        private string userSelectedSaveFolder = Settings.Default.userSelectedSaveFolder;
        private string userSelectedBackupFolder = Settings.Default.userSelectedBackupFolder;
        private string timeFormat = Settings.Default.userSelectedTimeFormat;


        // Method to save a new backup folder path
        public static void SaveBackupFolderPath(string newPath)
        {
            Settings.Default.userSelectedBackupFolder = newPath;
            Settings.Default.Save();
        }
        // Method to save a new savegame folder path
        public static void SaveGameFolderPath(string newPath)
        {
            Settings.Default.userSelectedSaveFolder = newPath;
            Settings.Default.Save();
        }

        // Method to load the saved backup folder path
        public static string GetBackupFolderPath()
        {
            if (Settings.Default.userSelectedBackupFolder == null) { 
                return Settings.Default.defaultBackupFolder;
            }
            else
            {
                return Settings.Default.userSelectedBackupFolder;
            }
        }
        public static string GetSaveGameFolderPath()
        {
            if (Settings.Default.userSelectedSaveFolder == null)
            {
                return Settings.Default.defaultSaveFolder;
            }
            else
            {
                return Settings.Default.userSelectedSaveFolder;
            }
        }

        // Method to save the selected time format
        public static void SaveTimeFormat(string newFormat)
        {
            
            Settings.Default.userSelectedTimeFormat = newFormat;
            Settings.Default.Save();
        }

        // Method to load the selected time format
        public static string GetTimeFormat()
        {
            if (string.IsNullOrEmpty(Settings.Default.userSelectedTimeFormat))
            {
                return Settings.Default.defaultTimeFormat;
            }
            else
            {
                return Settings.Default.userSelectedTimeFormat;
            }
        }

        // Method to validate folder paths in the settings
        public static void ValidateFoldersPaths(Action<string> showError)
        {
            string backupPath = GetBackupFolderPath();
            if (!Directory.Exists(backupPath))
            {
                showError($"The backup folder path {backupPath} does not exist.");
            }
            string restorePath = GetSaveGameFolderPath();
            if (!Directory.Exists(restorePath))
            {
                showError($"The restore folder path {restorePath} does not exist.");
            }
        }

    }
}
