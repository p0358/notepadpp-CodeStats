using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace CodeStats
{
    internal static class Constants
    {
        internal const string PluginName = "Code::Stats";
        internal const string PluginNameSafe = "CodeStats";
        internal const string PluginKey = "notepadpp-CodeStats";
        internal static string PluginVersion = string.Format("{0}.{1}.{2}", CodeStatsPackage.CoreAssembly.Version.Major, CodeStatsPackage.CoreAssembly.Version.Minor, CodeStatsPackage.CoreAssembly.Version.Build);
        internal static string PluginUserAgent = string.Format("code-stats-notepadpp/{0}", PluginVersion);
        internal const string EditorName = "notepadpp";
        internal static string EditorVersion {
            get
            {
                var ver = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETNPPVERSION, 0, 0);
                return ver.ToString();
            }
        }

        internal static string ApiMyPulsesEndpoint = "https://codestats.net/api/my/pulses";

        internal static string OSVersionString = System.Environment.OSVersion.VersionString;
        internal static int OSVersionBuild = System.Environment.OSVersion.Version.Build;

        internal enum DetectionType {
            EXTENSION_MAPPING,
            LEXER_LANGUAGE,
            CUSTOM_MAPPING
        };
        
        internal static Func<string> LatestPluginVersion = () =>
        {
            var regex = new Regex(@"\[assembly: AssemblyFileVersion\(\""(([0-9]+\.?){3})\""\)\]");

            var client = new WebClient { Proxy = CodeStatsPackage.GetProxy() };
            client.Headers[HttpRequestHeader.UserAgent] = Constants.PluginUserAgent;

            try
            {
                var about = client.DownloadString("https://raw.githubusercontent.com/p0358/notepadpp-CodeStats/master/CodeStats/Properties/AssemblyInfo.cs");
                var match = regex.Match(about);

                if (match.Success)
                {
                    /*var grp1 = match.Groups[2];
                    var regexVersion = new Regex("([0-9]+)");
                    var match2 = regexVersion.Matches(grp1.Value);
                    return string.Format("{0}.{1}.{2}", match2[0].Value, match2[1].Value, match2[2].Value);*/
                    return match.Groups[1].Value;
                }
                else
                {
                    Logger.Warning("Could not auto-resolve Code::Stats plugin version");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when checking current Code::Stats plugin version", ex);
            }
            return string.Empty;
        };
    }
}
