using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using Task = System.Threading.Tasks.Task;
using System.Collections.Concurrent;
using System.Collections;
using System.Timers;
using System.Web.Script.Serialization;
using System.IO;
using System.Collections.Generic;

using Kbg.NppPluginNET.PluginInfrastructure;
//using Newtonsoft.Json;

namespace CodeStats
{
    class CodeStatsPackage
    {
        #region Properties
        internal const string PluginName = Constants.PluginName;

        static int idMyDlg = -1;
        static Bitmap tbBmp = Properties.Resources.CodeStats;
        
        static ConfigFile _CodeStatsConfigFile;
        static CodeStats.Forms.SettingsForm _settingsForm;
        static string _lastStatusBarDocTypeText;
        static DateTime _lastPulse = DateTime.Now;
        static DateTime _lastActivity = DateTime.Now;
        static int pulseFrequency = 10000; // ms

        public static bool Debug;
        public static string ApiKey;
        public static string Proxy;
        public static bool Stats;
        public static string Guid;

        public static bool _reportedStats = false;

        //public static string currentLangName = ""; // N++ (ex. HTML, not compatible with Code::Stats names everywhere)
        public static string currentLangDesc = ""; // N++ (ex. Hyper Text Markup Language)
        public static string currentLanguage = ""; // Code::Stats
        public static int currentCount = 0;
        public static Pulse currentPulse;

        static bool nppStarted = false;

        static Dictionary<string, string> extensionMapping;

        private static ConcurrentQueue<Pulse> pulseQueue = new ConcurrentQueue<Pulse>();
        private static System.Timers.Timer timer = new System.Timers.Timer();
        #endregion

        internal static void CommandMenuInit()
        {
            
            // must add menu item in foreground thread
            PluginBase.SetCommand(0, "Code::Stats settings", SettingsPopup, new ShortcutKey(false, false, false, Keys.None));
            idMyDlg = 0;

            // finish initializing in background thread
            Task.Run(() =>
            {
                InitializeAsync();
            });
        }

        private static void InitializeAsync()
        {
            try
            {
                // Delete existing log file to save space
                Logger.Delete();
            }
            catch { }

            try
            {
                Logger.Info(string.Format("Initializing Code::Stats v{0}", Constants.PluginVersion));

                //Logger.Debug(Assembly.GetExecutingAssembly().GetManifestResourceNames().ToString());
                //Logger.Debug(Assembly.GetExecutingAssembly().GetName().Name);

                Assembly _assembly;
                StreamReader _textStreamReader;
                Stream _stream;
                string extensionMappingJson;

                try
                {
                    var client = new WebClient { Proxy = CodeStatsPackage.GetProxy() };

                    try
                    {
                        extensionMappingJson = client.DownloadString("https://raw.githubusercontent.com/p0358/notepadpp-CodeStats/master/CodeStats/Resources/extension_mapping.json");
                        if (!extensionMappingJson.Trim().StartsWith("{") || !extensionMappingJson.Trim().EndsWith("}"))
                        {
                            extensionMappingJson = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        extensionMappingJson = string.Empty;
                        Logger.Error("Exception when trying to download latest extension mappings, using local ones instead", ex);
                    }
                }
                catch
                {
                    extensionMappingJson = string.Empty;
                }

                if (String.IsNullOrWhiteSpace(extensionMappingJson)) 
                {
                    // Load precompiled/included extension mapping
                    _assembly = Assembly.GetExecutingAssembly();
                    _stream = _assembly.GetManifestResourceStream("CodeStats.Resources.extension_mapping.json");
                    _textStreamReader = new StreamReader(_stream);
                    extensionMappingJson = _textStreamReader.ReadToEnd();
                }

                Logger.Debug("Extension mapping JSON: " + extensionMappingJson);

                //var json = "{\"id\":\"13\", \"value\": true}";
                var serializer = new JavaScriptSerializer();
                //var table = serializer.Deserialize<dynamic>(json);
                //Dictionary<string, string> values = serializer.Deserialize<Dictionary<string, string>>(json);

                extensionMapping = serializer.Deserialize<Dictionary<string, string>>(extensionMappingJson);

                //Logger.Debug(values["id"]);
                //Logger.Debug(table["value"]);
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading extension mappings!", ex);
            }

            try
            {

                // Settings Form
                _settingsForm = new CodeStats.Forms.SettingsForm();
                _settingsForm.ConfigSaved += SettingsFormOnConfigSaved;

                // Load config file
                _CodeStatsConfigFile = new ConfigFile();
                GetSettings();

                try
                {
                    // Check for updates
                    string latest = Constants.LatestPluginVersion();
                    if (Constants.PluginVersion != latest && !String.IsNullOrWhiteSpace(latest))
                    {
                        MessageBox.Show("There is Code::Stats plugin update available!\nDownload it from Plugin Manager or GitHub.", "Code::Stats");
                    }
                }
                catch { }

                if (string.IsNullOrEmpty(ApiKey))
                {
                    Stats = false; // Disable stats reporting for this session/launch
                    PromptApiKey(); // Prompt for api key if not already set
                } 

                currentPulse = new Pulse();

                // setup timer to process queued pulses
                timer.Interval = pulseFrequency;
                timer.Elapsed += ProcessPulses;
                timer.Start();

                if (Stats)
                {
                    ReportStats();
                }

                Logger.Info(string.Format("Finished initializing Code::Stats v{0}", Constants.PluginVersion));
            }
            catch (Exception ex)
            {
                Logger.Error("Error Initializing CodeStats", ex);
            }
        }

        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        internal static void SetStatusBarDocType(string str)
        {

            //IntPtr pStr = Marshal.AllocHGlobal(Marshal.SizeOf(str));
            //Marshal.StructureToPtr(str, pStr, false);
            string strr = @"" + str;
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETSTATUSBAR, /*(IntPtr)NppMsg.STATUSBAR_DOC_TYPE*/0, strr);
            //Marshal.FreeHGlobal(pStr);
        }

        public static void OnNotification(ScNotification notification)
        {
            // TODO: NPPN_SHUTDOWN - save unpulsed to config (or do it in plugin unload sequence rather on bottom of this file)
            if (notification.Header.Code == (uint)NppMsg.NPPN_LANGCHANGED) // Does not seem to be triggered?
            {
                //currentLangDesc = GetCurrentLangDesc(); - below does this
                UpdateStatusbar();
            }
            if (notification.Header.Code == (uint)NppMsg.NPPN_READY) // IMPORTANT: It triggers SCN_MODIFIED for each opened file before this
            {
                nppStarted = true;
                UpdateStatusbar();
            }
            if (nppStarted && (notification.Header.Code == (uint)SciMsg.SCN_MODIFIED || notification.Header.Code == (uint)NppMsg.NPPN_TBMODIFICATION))
            {
                UpdateStatusbar();
            }

            /*if (notification.Header.Code == (uint)NppMsg.NPPN_FILESAVED) // It does count as 1 XP in Visual Studio Code, though not sure if intended!
            {
                HandleActivity(GetCurrentFile(), true);
            }*/

            // (notification.ModificationType & ((int)SciMsg.SC_MOD_INSERTTEXT | (int)SciMsg.SC_MOD_DELETETEXT) == ((int)SciMsg.SC_MOD_INSERTTEXT | (int)SciMsg.SC_MOD_DELETETEXT))

            /*if (  notification.Header.Code == (uint)SciMsg.SCN_MODIFIED &&
                ( ((notification.ModificationType & (int)SciMsg.SC_MOD_INSERTTEXT) == (int)SciMsg.SC_MOD_INSERTTEXT)
                  || ((notification.ModificationType & (int)SciMsg.SC_MOD_DELETETEXT) == (int)SciMsg.SC_MOD_DELETETEXT) )
                && ((notification.ModificationType & (int)SciMsg.SC_PERFORMED_USER) == (int)SciMsg.SC_PERFORMED_USER)  )
            {
                Logger.Info("1. File: " + GetCurrentFile() + ", char: " + notification.character + ", nppstarted: " + nppStarted.ToString() + ", flags: " + notification.ModificationType.ToString("X"));
            }*/ // Too unstable - 0x2012 (ModificationType) might be used for deleting of selected text, but it's still nowhere near good

                // TODO: Could use SCN_MODIFIED perhaps, but check for file status with NPPM_READY (don't count before this one), 
                // NPPM_FILE_BEFORE_LOAD, (NPPM_FILE_LOAD_FAILED), NPPM_FILE_OPENED (then start counting?), NPPM_FILEBEFORECLOSE [http://docs.notepad-plus-plus.org/index.php/Messages_And_Notifications]

            if (notification.Header.Code == (uint)SciMsg.SCN_CHARADDED) // our best bet
            {
                HandleActicity();
                Logger.Debug("SCN_CHARADDED - File: " + GetCurrentFile() + ", char: " + notification.character + ", lang: " + GetCurrentLanguage());
            }

            int SC_PERFORMED_USER_AND_SC_MOD_DELETETEXT = (int)SciMsg.SC_PERFORMED_USER | (int)SciMsg.SC_MOD_DELETETEXT;
            if (nppStarted && notification.Header.Code == (uint)SciMsg.SCN_MODIFIED && ((notification.ModificationType & SC_PERFORMED_USER_AND_SC_MOD_DELETETEXT) == SC_PERFORMED_USER_AND_SC_MOD_DELETETEXT))
            {
                // Looks like we can use this to track deleted stuff once Notepad++ is started and ready
                // It doesn't trigger on file close either, unlike on open with SC_MOD_INSERTTEXT, so we only use this, and SCN_CHARADDED for inserts
                // It will skip Ctrl+V if it wasn't pasted on some existing text, but never mind, it is still counting the most we want
                // And we would not like it to count random file opens or other actions in
                HandleActicity();
                Logger.Debug("SC_PERFORMED_USER & SC_MOD_DELETETEXT - File: " + GetCurrentFile() + ", char: " + notification.character + ", flags: " + notification.ModificationType.ToString("X"));
            }

            if (notification.Header.Code == (uint)SciMsg.SCEN_CHANGE) // Does not seem to be ever triggered (ah, right, GTK+ only it seems)
            {
                Logger.Debug("SCEN_CHANGE - File: " + GetCurrentFile() + ", char: " + notification.character + ", lang: " + GetCurrentLanguage());
            }
            // http://docs.notepad-plus-plus.org/index.php/Messages_And_Notifications
            // http://www.scintilla.org/ScintillaDoc.html
            // https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net/blob/master/Demo%20Plugin/NppManagedPluginDemo/Demo.cs
        }

        public static void UpdateStatusbar()
        {
            /*if (String.IsNullOrWhiteSpace(currentLangDesc))*/
            currentLangDesc = GetCurrentLangDesc(); // Unfortunately it doesn't get changed on notification
            string append = @"";
            if (!String.IsNullOrWhiteSpace(currentLangDesc)) append = @" | " + currentLangDesc;
            string newStatusBarDocTypeText = @"✎C::S " + currentCount.ToString() + append;
            if (_lastStatusBarDocTypeText != newStatusBarDocTypeText) // Let's not call it when it's not needed (nothing changed), we will earn some performance hopefully
            {
                SetStatusBarDocType(newStatusBarDocTypeText);
            }
            _lastStatusBarDocTypeText = newStatusBarDocTypeText;
        }

        public static void HandleActicity()
        {
            if (!nppStarted) return;

            currentCount += 1;

            UpdateStatusbar();

            currentLanguage = GetCurrentLanguage();
            currentPulse.addXpForLanguage(currentLanguage, 1);

            /*var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string json = jsonSerializer.Serialize(currentPulse);
            Logger.Debug(json);*/
            _lastActivity = DateTime.Now;
        }

        private static void ProcessPulses(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    ProcessPulses();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error processing pulses", ex);
                }
            });
        }

        private static void ProcessPulses()
        {
            if (!currentPulse.isEmpty() && EnoughTimePassed(DateTime.Now))
            {
                pulseQueue.Enqueue(currentPulse);
                currentPulse = new Pulse();
                currentCount = 0;

                if (String.IsNullOrWhiteSpace(ApiKey))
                {
                    Logger.Debug("No API key - cannot pulse!");
                    return;
                }

                var client = new WebClient { Proxy = CodeStatsPackage.GetProxy() };
                var jsonSerializer = new JavaScriptSerializer();

                string URL;
                if (String.IsNullOrWhiteSpace(_CodeStatsConfigFile.ApiUrl))
                {
                    URL = Constants.ApiMyPulsesEndpoint;
                } else
                {
                    URL = _CodeStatsConfigFile.ApiUrl;
                }
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Accept] = "*/*";
                client.Headers["X-API-Token"] = ApiKey;

                Pulse result;
                while (pulseQueue.TryDequeue(out result))
                {
                    if (!result.isEmpty())
                    {
                        bool error = false;
                        // Try to pulse to API
                        try
                        {
                            string json = jsonSerializer.Serialize(result);
                            Logger.Debug("Pulsing " + json);
                            string HtmlResult = client.UploadString(URL, json);
                            _lastPulse = DateTime.Now;
                            if (!HtmlResult.Contains(@"""ok""") && !HtmlResult.Contains(@"success"))
                            {
                                error = true;
                                Logger.Error(@"Error pulsing, response does not contain ""ok"" or ""success"": " + HtmlResult);
                            }
                        }
                        catch (WebException ex)
                        {
                            error = true;
                            Logger.Error("Could not pulse. Are you behind a proxy? Try setting a proxy in Code::Stats settings with format https://user:pass@host:port. Exception Traceback:", ex);
                        }
                        catch (Exception ex)
                        {
                            error = true;
                            Logger.Error("Error pulsing. Exception Traceback:", ex);
                        }

                        if (error)
                        {
                            pulseQueue.Enqueue(result); // Requeue, since we failed to pulse
                            return;
                        }

                    }
                }
            }

            UpdateStatusbar();
        }

        public static string GetCurrentFile()
        {
            var currentFile = new StringBuilder(Win32.MAX_PATH);
            return
                (int)Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLCURRENTPATH, 0, currentFile) != -1
                    ? currentFile.ToString()
                    : null;
        }

        public static LangType GetCurrentLangType()
        {
            LangType docType = LangType.L_TEXT;
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTLANGTYPE, 0, ref docType);
            return docType;
        }

        public static string GetCurrentLangName()
        {
            return GetLangName(GetCurrentLangType());
        }

        public static string GetLangName(LangType langType) // N++ // TODO: Create local dictionary with those
        {
            var languageName = new StringBuilder(Win32.MAX_PATH);
            return
                (int)Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETLANGUAGENAME, (IntPtr)langType, languageName) != -1
                    ? languageName.ToString()
                    : null;
        }

        public static string GetCurrentLangDesc()
        {
            return GetLangDesc(GetCurrentLangType());
        }

        public static string GetLangDesc(LangType langType) // N++ // TODO: Create local dictionary with those
        {
            var languageDesc = new StringBuilder(Win32.MAX_PATH);
            return
                (int)Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETLANGUAGEDESC, (IntPtr)langType, languageDesc) != -1
                    ? languageDesc.ToString()
                    : null;
        }

        public static string GetCurrentLanguage() // Code::Stats
        {
            LangType langType = GetCurrentLangType();
            string currentFile = GetCurrentFile();
            string extension = "";
            extension = Path.GetExtension(currentFile).TrimStart('.');

            bool found = false;
            string language = "";
            if (!String.IsNullOrWhiteSpace(extension))
            {
                /*if (extensionMapping.TryGetValue(extension, out language))
                {
                    // Found
                    found = true;
                }
                else
                {
                    // Not found
                    found = false;
                }*/// - doesn't work for some reason
                try
                {
                    language = extensionMapping[extension];
                    found = true;
                }
                catch (KeyNotFoundException)
                {
                    Logger.Info("Key = \"" + extension + "\" is not found.");
                    found = false;
                }
            }

            if (!found)
            {
                switch (langType)
                {
                    case LangType.L_ADA: language = "Ada"; break;
                    //case LangType.L_ASCII: language = "ASCII"; break; // maybe Plain text?
                    case LangType.L_ASM: language = "Assembler"; break;
                    case LangType.L_ASP: language = "ASP"; break;
                    case LangType.L_AU3: language = "AutoIt"; break;
                    case LangType.L_BASH: language = "Bash"; break; // or Shell Script, probably called Shell in NPP
                    case LangType.L_BATCH: language = "Batch"; break;
                    case LangType.L_C: language = "C"; break;
                    case LangType.L_CAML: language = "Caml"; break;
                    case LangType.L_CMAKE: language = "CMake"; break;
                    case LangType.L_COBOL: language = "COBOL"; break;
                    case LangType.L_COFFEESCRIPT: language = "CoffeeScript"; break;
                    case LangType.L_CPP: language = "C++"; break;
                    case LangType.L_CS: language = "C#"; break;
                    case LangType.L_CSS: language = "CSS"; break;
                    case LangType.L_D: language = "D"; break;
                    case LangType.L_DIFF: language = "Diff"; break;
                    //case LangType.L_EXTERNAL: language = ""; break;
                    case LangType.L_FLASH: language = "Flash"; break;
                    case LangType.L_FORTRAN: language = "Fortran"; break; // fixed?
                    case LangType.L_FORTRAN_77: language = "Fortran"; break; // free?
                    case LangType.L_GUI4CLI: language = "Gui4Cli"; break;
                    case LangType.L_HASKELL: language = "Haskell"; break;
                    case LangType.L_HTML: language = "HTML"; break;
                    case LangType.L_INI: language = "Ini"; break;
                    case LangType.L_INNO: language = "INNO"; break;
                    case LangType.L_JAVA: language = "Java"; break;
                    case LangType.L_JAVASCRIPT: language = "JavaScript"; break;
                    case LangType.L_JS: language = "JavaScript"; break;
                    case LangType.L_JSON: language = "JSON"; break;
                    case LangType.L_JSP: language = "JSP"; break;
                    case LangType.L_KIX: language = "KIXtart"; break;
                    case LangType.L_LISP: language = "LISP"; break;
                    case LangType.L_LUA: language = "Lua"; break;
                    case LangType.L_MAKEFILE: language = "Makefile"; break;
                    case LangType.L_MATLAB: language = "Matlab"; break;
                    case LangType.L_NSIS: language = "NSIS"; break;
                    case LangType.L_OBJC: language = "Objective-C"; break;
                    case LangType.L_PASCAL: language = "Pascal"; break;
                    case LangType.L_PERL: language = "Perl"; break;
                    case LangType.L_PHP: language = "PHP"; break;
                    case LangType.L_POWERSHELL: language = "PowerShell"; break;
                    case LangType.L_PROPS: language = "Properties"; break;
                    case LangType.L_PS: language = "PostScript"; break;
                    case LangType.L_PYTHON: language = "Python"; break;
                    case LangType.L_R: language = "R"; break;
                    case LangType.L_RC: language = "Resource"; break;
                    case LangType.L_RUBY: language = "Ruby"; break;
                    case LangType.L_SCHEME: language = "Scheme"; break;
                    //case LangType.L_SEARCHRESULT: language = ""; break; // it should be fine as plaintext
                    case LangType.L_SMALLTALK: language = "Smalltalk"; break;
                    case LangType.L_SQL: language = "SQL"; break;
                    case LangType.L_TCL: language = "TCL"; break;
                    case LangType.L_TEX: language = "TeX"; break;
                    case LangType.L_VB: language = "Visual Basic"; break;
                    case LangType.L_VERILOG: language = "Verilog"; break;
                    case LangType.L_VHDL: language = "VHDL"; break;
                    case LangType.L_XML: language = "XML"; break;
                    case LangType.L_YAML: language = "YAML"; break;
                    case LangType.L_TEXT:
                    case LangType.L_USER: // User defined language
                    default:
                        language = "Plain text";
                        break;
                }
            }

            return language;
        }

        public static bool EnoughTimePassed(DateTime now)
        {
            return (_lastActivity < now.AddMilliseconds(-1 * pulseFrequency)) && (_lastPulse < now.AddMilliseconds(-1 * pulseFrequency));
        }

        private static void SettingsFormOnConfigSaved(object sender, EventArgs eventArgs)
        {
            GetSettings();
        }

        private static void GetSettings()
        {
            _CodeStatsConfigFile.Read();
            ApiKey = _CodeStatsConfigFile.ApiKey;
            Debug = _CodeStatsConfigFile.Debug;
            Proxy = _CodeStatsConfigFile.Proxy;
            Stats = _CodeStatsConfigFile.Stats;
            CodeStatsPackage.Guid = _CodeStatsConfigFile.Guid;
        }

        private static void PromptApiKey()
        {
            Logger.Info("Please input your api key into the Code::Stats window.");
            var form = new CodeStats.Forms.ApiKeyForm();
            form.ShowDialog();
        }

        private static void SettingsPopup()
        {
            _settingsForm.ShowDialog();
        }

        public static void ReportStats()
        {
            var client = new WebClient { Proxy = CodeStatsPackage.GetProxy() };
            string HtmlResult = client.DownloadString("https://p0358.cf/codestats/report.php?pluginver=" + Constants.PluginVersion + "&cid=" + CodeStatsPackage.Guid); // expected response: ok
            if (HtmlResult.Contains("ok")) _reportedStats = true;
        }

        private static string ToUnixEpoch(DateTime date)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timestamp = date - epoch;
            long seconds = Convert.ToInt64(Math.Floor(timestamp.TotalSeconds));
            string milliseconds = timestamp.ToString("ffffff");
            return string.Format("{0}.{1}", seconds, milliseconds);
        }

        public static WebProxy GetProxy()
        {
            WebProxy proxy = null;

            try
            {
                var proxyStr = Proxy;

                // Regex that matches proxy address with authentication
                var regProxyWithAuth = new Regex(@"\s*(https?:\/\/)?([^\s:]+):([^\s:]+)@([^\s:]+):(\d+)\s*");
                var match = regProxyWithAuth.Match(proxyStr);

                if (match.Success)
                {
                    var username = match.Groups[2].Value;
                    var password = match.Groups[3].Value;
                    var address = match.Groups[4].Value;
                    var port = match.Groups[5].Value;

                    var credentials = new NetworkCredential(username, password);
                    proxy = new WebProxy(string.Join(":", new string[] { address, port }), true, null, credentials);

                    Logger.Debug("A proxy with authentication will be used.");
                    return proxy;
                }

                // Regex that matches proxy address and port(no authentication)
                var regProxy = new Regex(@"\s*(https?:\/\/)?([^\s@]+):(\d+)\s*");
                match = regProxy.Match(proxyStr);

                if (match.Success)
                {
                    var address = match.Groups[2].Value;
                    var port = int.Parse(match.Groups[3].Value);

                    proxy = new WebProxy(address, port);

                    Logger.Debug("A proxy will be used.");
                    return proxy;
                }

                Logger.Debug("No proxy will be used. It's either not set or badly formatted.");
            }
            catch (Exception ex)
            {
                Logger.Error("Exception while parsing the proxy string from CodeStats config file. No proxy will be used.", ex);
            }

            return proxy;
        }

        internal static void RecursiveDelete(string folder)
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch { /* ignored */ }
            try
            {
                File.Delete(folder);
            }
            catch { /* ignored */ }
        }

        internal static class CoreAssembly
        {
            static readonly Assembly Reference = typeof(CoreAssembly).Assembly;
            public static readonly Version Version = Reference.GetName().Version;
        }

        internal static void PluginCleanUp()
        {
            nppStarted = false;

            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= ProcessPulses;
                timer.Dispose();
                timer = null;

                // make sure the queue is empty
                ProcessPulses();
            }
        }
    }
}