// #define SAVE_LOG_IN_TEMP_INSTEAD_OF_PLUGIN_DIR

using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CodeStats
{
    internal enum LogLevel
    {
        Debug = 1,
        Info,
        Warning,
        HandledException
    };

    static class Logger
    {
        internal static string configDir;

        internal static void Debug(string msg)
        {
            if (!CodeStatsPackage.Debug)
                return;

            Log(LogLevel.Debug, msg);
        }

        internal static void Info(string msg)
        {
            Log(LogLevel.Info, msg);
        }

        internal static void Warning(string msg)
        {
            Log(LogLevel.Warning, msg);
        }

        internal static void Error(string msg, Exception ex = null)
        {
            var exceptionMessage = string.Format("{0}: {1}", msg, ex);

            Log(LogLevel.HandledException, exceptionMessage);
        }

        internal static void Log(LogLevel level, string msg)
        {
            try
            {
                var writer = Setup();
                if (writer == null) return;

                writer.WriteLine("[Code::Stats {0} {1}] {2}", Enum.GetName(level.GetType(), level), DateTime.Now.ToString("HH:mm:ss"), msg);            
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error writing to log file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //MessageBox.Show(string.Format("{0}\\{1}.log", configDir, Constants.PluginName), "Error writing to log file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static StreamWriter Setup()
        {
            //var configDir = Dependencies.AppDataDirectory;
            if (String.IsNullOrWhiteSpace(configDir))
            {
#if SAVE_LOG_IN_TEMP_INSTEAD_OF_PLUGIN_DIR
                configDir = System.Environment.GetEnvironmentVariable("TEMP");
#else // BACKWARD COMPATIBLE
                // get path of plugin configuration
                StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
                configDir = sbIniFilePath.ToString();
#endif
            }
            if (string.IsNullOrWhiteSpace(configDir)) return null;

            var filename = string.Format("{0}\\{1}.log", configDir, "CodeStats");
            var writer = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write));
            return writer;
        }

        public static void Delete()
        {
            //var configDir = Dependencies.AppDataDirectory;
            if (String.IsNullOrWhiteSpace(configDir))
            {
#if SAVE_LOG_IN_TEMP_INSTEAD_OF_PLUGIN_DIR
                configDir = System.Environment.GetEnvironmentVariable("TEMP");
#else // BACKWARD COMPATIBLE
                // get path of plugin configuration
                StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
                configDir = sbIniFilePath.ToString();
#endif
            }
            if (string.IsNullOrWhiteSpace(configDir)) return;

            var filename = string.Format("{0}\\{1}.log", configDir, "CodeStats");
            File.Delete(filename);
        }
    }
}
