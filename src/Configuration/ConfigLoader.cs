using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileWatcher.src.Configuration
{
    public class ConfigLoader
    {
        private static readonly string _configLocation = @"..\..\..\..\Config\FileSystemWatcher.config";
        private static XElement _config;

        public static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void LoadConfig()
        {
            string configStr;
            try
            {
                configStr = File.ReadAllText(_configLocation);
            }
            catch (Exception ex)
            {
                throw new IOException("Error reading in config file.", ex);
            }

            try
            {
                _config = XElement.Parse(configStr);
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Error Parsing configuration XML. Path: {_configLocation}", e);
            }
        }

        public static XElement GetConfigItem()
        {
            return _config;
        }
    }
}
