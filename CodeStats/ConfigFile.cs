using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeStats
{
    public class ConfigFile
    {
        internal string ApiKey { get; set; }
        internal string ApiUrl { get; set; }
        internal string Proxy { get; set; }
        internal bool Stats { get; set; }
        internal string Guid { get; set; }
        internal bool Debug { get; set; }
        internal bool UseExtensionMapping { get; set; }
        internal bool UseLexerLanguage { get; set; }
        internal Constants.DetectionType DetectionPriority { get; set; }
        internal bool UseCustomMapping { get; set; }

        internal List<Constants.DetectionType> DetectionOrder { get; set; }

        private readonly string _configFilepath;

        internal ConfigFile()
        {
            _configFilepath = GetConfigFilePath();
            Read();
        }

        internal void Read()
        {
            var ret = new StringBuilder(2083);

            ApiKey = NativeMethods.GetPrivateProfileString("settings", "api_key", "", ret, 2083, _configFilepath) > 0
                ? ret.ToString()
                : string.Empty;

            ApiUrl = NativeMethods.GetPrivateProfileString("settings", "api_url", "", ret, 2083, _configFilepath) > 0
                ? ret.ToString()
                : string.Empty;

            Proxy = NativeMethods.GetPrivateProfileString("settings", "proxy", "", ret, 2083, _configFilepath) > 0
                ? ret.ToString()
                : string.Empty;

            if (NativeMethods.GetPrivateProfileString("settings", "stats", "true", ret, 2083, _configFilepath) > 0)
            {
                bool stats;
                if (bool.TryParse(ret.ToString(), out stats))
                    Stats = stats;
            }
            else Stats = true;

            this.Guid = NativeMethods.GetPrivateProfileString("settings", "guid", "", ret, 2083, _configFilepath) > 0
                ? ret.ToString()
                : System.Guid.NewGuid().ToString();

            // ReSharper disable once InvertIf
            if (NativeMethods.GetPrivateProfileString("settings", "debug", "", ret, 2083, _configFilepath) > 0)
            {
                bool debug;
                if (bool.TryParse(ret.ToString(), out debug))
                    Debug = debug;
            }

            #region detection order settings

            if (NativeMethods.GetPrivateProfileString("detection_order", "use_extension_mapping", "true", ret, 2083, _configFilepath) > 0)
            {
                bool extmap;
                if (bool.TryParse(ret.ToString(), out extmap))
                    UseExtensionMapping = extmap;
            }
            else UseExtensionMapping = true;

            if (NativeMethods.GetPrivateProfileString("detection_order", "use_lexer_language", "true", ret, 2083, _configFilepath) > 0)
            {
                bool lexlag;
                if (bool.TryParse(ret.ToString(), out lexlag))
                    UseLexerLanguage = lexlag;
            }
            else UseLexerLanguage = true;

            this.DetectionPriority = NativeMethods.GetPrivateProfileString("detection_order", "priority", ((int)Constants.DetectionType.EXTENSION_MAPPING).ToString(), ret, 2083, _configFilepath) > 0
                ? (Constants.DetectionType)System.Int32.Parse(ret.ToString())
                : Constants.DetectionType.EXTENSION_MAPPING;

            RefreshDetectionOrder();

            if (NativeMethods.GetPrivateProfileString("detection_order", "use_custom_mapping", "false", ret, 2083, _configFilepath) > 0)
            {
                bool cusmap;
                if (bool.TryParse(ret.ToString(), out cusmap))
                    UseCustomMapping = cusmap;
            }
            else UseCustomMapping = false;

            #endregion
        }

        private List<Constants.DetectionType> GetDetectionOrder()
        {
            List<Constants.DetectionType> list = new List<Constants.DetectionType>();

            if (UseCustomMapping)
            {
                list.Add(Constants.DetectionType.CUSTOM_MAPPING);
            }

            if (UseExtensionMapping && UseLexerLanguage)
            {
                if (DetectionPriority == Constants.DetectionType.LEXER_LANGUAGE)
                {
                    list.Add(Constants.DetectionType.LEXER_LANGUAGE);
                    list.Add(Constants.DetectionType.EXTENSION_MAPPING);
                }
                else
                {
                    list.Add(Constants.DetectionType.EXTENSION_MAPPING);
                    list.Add(Constants.DetectionType.LEXER_LANGUAGE);
                }
            }
            else
            {
                if (UseExtensionMapping) list.Add(Constants.DetectionType.EXTENSION_MAPPING);
                if (UseLexerLanguage) list.Add(Constants.DetectionType.LEXER_LANGUAGE);
            }

            return list;
        }

        internal void RefreshDetectionOrder()
        {
            DetectionOrder = GetDetectionOrder();
        }

        internal void Save()
        {
            if (!string.IsNullOrWhiteSpace(ApiKey))
                NativeMethods.WritePrivateProfileString("settings", "api_key", ApiKey.Trim(), _configFilepath);

            if (ApiUrl == Constants.ApiEndpoint || string.IsNullOrWhiteSpace(ApiUrl))
            {
                NativeMethods.WritePrivateProfileString("settings", "api_url", string.Empty, _configFilepath);
            }
            else if (!string.IsNullOrWhiteSpace(ApiUrl))
            {
                NativeMethods.WritePrivateProfileString("settings", "api_url", ApiUrl.Trim(), _configFilepath);
            }

            NativeMethods.WritePrivateProfileString("settings", "stats", Stats.ToString().ToLower(), _configFilepath);
            NativeMethods.WritePrivateProfileString("settings", "guid", this.Guid, _configFilepath);
            NativeMethods.WritePrivateProfileString("settings", "proxy", Proxy.Trim(), _configFilepath);
            NativeMethods.WritePrivateProfileString("settings", "debug", Debug.ToString().ToLower(), _configFilepath);
           
            NativeMethods.WritePrivateProfileString("detection_order", "use_extension_mapping", UseExtensionMapping.ToString().ToLower(), _configFilepath);
            NativeMethods.WritePrivateProfileString("detection_order", "use_lexer_language", UseLexerLanguage.ToString().ToLower(), _configFilepath);
            NativeMethods.WritePrivateProfileString("detection_order", "priority", ((int)DetectionPriority).ToString(), _configFilepath);
            NativeMethods.WritePrivateProfileString("detection_order", "use_custom_mapping", UseCustomMapping.ToString().ToLower(), _configFilepath);
        }

        static string GetConfigFilePath()
        {
            //var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // get path of plugin configuration
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            string iniFilePath = sbIniFilePath.ToString();

            return Path.Combine(iniFilePath, "CodeStats.ini");
        }
        
        public static string GetCustomExtensionMappingFilePath()
        {
            StringBuilder sbConfigFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbConfigFilePath);
            string configFilePath = sbConfigFilePath.ToString();

            return Path.Combine(configFilePath, "CodeStatsCustomExtensionMapping.json");
        }
        
        public static string GetUnsavedPulsesFilePath()
        {
            StringBuilder sbConfigFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbConfigFilePath);
            string configFilePath = sbConfigFilePath.ToString();

            return Path.Combine(configFilePath, "CodeStatsPendingPulses.json");
        }
    }
}