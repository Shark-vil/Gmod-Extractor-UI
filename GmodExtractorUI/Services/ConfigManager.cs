using GmaExtractorLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace GmodExtractorUI.Services
{
    public class ConfigManager
    {
        public static string FileSettingsPath;

        public static ConfigStructure ExtractPath;
        public static ConfigStructure GameFolderPath;
        public static ConfigStructure ContentPath;
        public static ConfigStructure SevenZipExePath;

        public static void Initialization()
        {
            string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string ConfigDirectoryPath = System.IO.Path.Combine(BaseDirectory, "Config");
            FileSettingsPath = System.IO.Path.Combine(ConfigDirectoryPath, "settings.cfg");

            Extractor.InitConfig();

            ExtractPath = new ConfigStructure
            {
                Key = "ExtractPath",
                Value = Extractor.ExtractPath
            };

            GameFolderPath = new ConfigStructure
            {
                Key = "GameFolderPath",
                Value = Extractor.GameFolderPath
            };

            ContentPath = new ConfigStructure
            {
                Key = "ContentPath",
                Value = Extractor.ContentPath
            };

            SevenZipExePath = new ConfigStructure
            {
                Key = "SevenZipExePath",
                Value = Extractor.SevenZipExePath
            };
        }

        public static void UpdateExtractPath(string NewValue)
        {
            ExtractPath.Value = NewValue;
            UpdateConfig();
        }

        public static void UpdateGameFolderPath(string NewValue)
        {
            GameFolderPath.Value = NewValue;
            UpdateConfig();
        }

        public static void UpdateContentPath(string NewValue)
        {
            ContentPath.Value = NewValue;
            UpdateConfig();
        }

        public static void UpdateSevenZipExePath(string NewValue)
        {
            SevenZipExePath.Value = NewValue;
            UpdateConfig();
        }

        public static void UpdateConfig()
        {
            ConfigFileManager.WriteConfig(FileSettingsPath, new List<ConfigStructure>
            {
                ExtractPath,
                GameFolderPath,
                ContentPath,
                SevenZipExePath
            });
        }
    }
}
