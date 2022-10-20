using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace CodeStats
{
    class CodeStatsPackage
    {
        #region Properties
        internal const string PluginName = Constants.PluginName;

        static int idMyDlg = -1;
        static Bitmap tbBmp = Properties.Resources.CodeStats;

        static ConfigFile _CodeStatsConfigFile;
        public static CodeStats.Forms.SettingsForm _settingsForm;
        public static CodeStats.Forms.ApiKeyForm _apikeyForm; // should be null if not needed
        static string _lastStatusBarDocTypeText;
        static DateTime _lastPulse = DateTime.Now;
        static DateTime _lastActivity = DateTime.Now;
        static int pulseFrequency = 10000; // ms

        public static bool Debug;
        public static string ApiKey;
        public static string ApiUrl;
        public static string Proxy;
        public static bool proxyChangePending = false;
        public static bool Stats;
        public static string Guid;
        public static List<Constants.DetectionType> DetectionOrder;

        public static bool _reportedStats = false;
        public static bool _hasAlreadyShownInvalidApiTokenMessage = false;

        //public static string currentLangName = ""; // N++ (ex. HTML, not compatible with Code::Stats names everywhere)
        public static string currentLangDesc = ""; // N++ (ex. Hyper Text Markup Language)
        public static string currentLanguage = ""; // Code::Stats
        public static int currentCount = 0;
        public static Pulse currentPulse;

        static bool nppStarted = false;

        static Dictionary<string, string> extensionMapping;
        static Dictionary<string, string> customExtensionMapping;

        private static ConcurrentQueue<Pulse> pulseQueue = new ConcurrentQueue<Pulse>();
        private static System.Timers.Timer timer = new System.Timers.Timer();

        public static Task pulseProcessor { get; set; }
        private static CancellationTokenSource pulseProcessor_tokensource;
        private static HttpClient pulseProcessor_client;
        private static HttpClientHandler pulseProcessor_httpClientHandler;

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

            //System.Diagnostics.Debugger.Launch();
        }

        private static void InitializeAsync()
        {
            if (System.Net.ServicePointManager.SecurityProtocol != 0)
            {
                // If the value is not set to 0 (SystemDefault), disable old protocols and make sure TLS 1.3, 1.2, 1.1 are enabled
                System.Net.ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
                System.Net.ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
                System.Net.ServicePointManager.SecurityProtocol |= (SecurityProtocolType)12288 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            }
            //System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)0; // SystemDefault, we don't use this since it's supported only since .NET 4.7

            try
            {
                // Delete existing log file to save space
                Logger.Delete();
            }
            catch { }

            Logger.Info(string.Format("Initializing Code::Stats v{0}", Constants.PluginVersion));

            try
            {
                currentPulse = new Pulse();

                // Load config file
                _CodeStatsConfigFile = new ConfigFile();
                GetSettings(true);

                Logger.Debug("Loaded config");

                LoadExtensionMapping();
                LoadCustomExtensionMapping();

                // Check for updates
                Task.Run(() =>
                {
                    try
                    {
                        string latest = Constants.LatestPluginVersion();
                        Logger.Debug("Latest version of the plugin online is: " + latest);
                        if (Constants.PluginVersion != latest && !String.IsNullOrWhiteSpace(latest))
                        {
                            MessageBox.Show("There is Code::Stats plugin update available!\nDownload it from Plugins Admin (if already available there) or GitHub.\nYour version: " + Constants.PluginVersion + "\nLatest: " + latest, "Code::Stats");
                        }
                    }
                    catch { }
                });


                if (string.IsNullOrEmpty(ApiKey))
                {
                    Stats = false; // Disable stats reporting for this session/launch
                    PromptApiKey(); // Prompt for API token if not already set
                }

                // setup timer to process queued pulses
                pulseProcessor_tokensource = new CancellationTokenSource();
                pulseProcessor_httpClientHandler = new HttpClientHandler();
                pulseProcessor_httpClientHandler.Proxy = GetProxy();
                proxyChangePending = false;
                pulseProcessor_client = new HttpClient(pulseProcessor_httpClientHandler);
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
                Logger.Error("Error Initializing Code::Stats", ex);
            }
        }

        private static void LoadExtensionMapping()
        {
            try
            {
                //Logger.Debug(Assembly.GetExecutingAssembly().GetManifestResourceNames().ToString());
                //Logger.Debug(Assembly.GetExecutingAssembly().GetName().Name);

                Assembly _assembly;
                StreamReader _textStreamReader;
                Stream _stream;
                string extensionMappingJson;
                var serializer = new JavaScriptSerializer();

                // Load precompiled/included extension mapping first
                _assembly = Assembly.GetExecutingAssembly();
                _stream = _assembly.GetManifestResourceStream("CodeStats.Resources.extension_mapping.json");
                _textStreamReader = new StreamReader(_stream);
                extensionMappingJson = _textStreamReader.ReadToEnd();
                extensionMapping = serializer.Deserialize<Dictionary<string, string>>(extensionMappingJson);

                Logger.Debug("Loaded local precompiled extension mapping");

                // fetch up-to-date mappings from network asynchronously
                Task.Run(() =>
                {
                    string extensionMappingJsonNew = "";
                    var serializer2 = new JavaScriptSerializer();
                    try
                    {

                        var client = new WebClient { Proxy = CodeStatsPackage.GetProxy() };
                        client.Headers[HttpRequestHeader.UserAgent] = Constants.PluginUserAgent;

                        try
                        {
                            extensionMappingJsonNew = client.DownloadString("https://raw.githubusercontent.com/p0358/notepadpp-CodeStats/master/CodeStats/Resources/extension_mapping.json").Trim();
                            if (!extensionMappingJsonNew.StartsWith("{") || !extensionMappingJson.EndsWith("}"))
                            {
                                extensionMappingJsonNew = string.Empty;
                                Logger.Error("Invalid response when trying to download latest extension mappings, using local ones instead");
                            }
                        }
                        catch (Exception ex)
                        {
                            extensionMappingJson = string.Empty;
                            Logger.Error("Exception when trying to download latest extension mappings, using local ones instead", ex);
                        } // update extension mapping JSON
                    }
                    catch
                    {
                        extensionMappingJson = string.Empty;
                    } // get webclient, set proxy, update extension mapping JSON

                    if (!String.IsNullOrWhiteSpace(extensionMappingJsonNew) && extensionMappingJsonNew != extensionMappingJson)
                    {
                        extensionMapping = serializer2.Deserialize<Dictionary<string, string>>(extensionMappingJson);
                        Logger.Debug("Loaded latest extension mapping JSON");
                    }
                    else Logger.Debug("There are no updates to extension mapping JSON");
                });

                //Logger.Debug("Extension mapping JSON: " + extensionMappingJson);

                //var json = "{\"id\":\"13\", \"value\": true}";
                //var serializer = new JavaScriptSerializer();
                //var table = serializer.Deserialize<dynamic>(json);
                //Dictionary<string, string> values = serializer.Deserialize<Dictionary<string, string>>(json);

                //extensionMapping = serializer.Deserialize<Dictionary<string, string>>(extensionMappingJson);

                //Logger.Debug(values["id"]);
                //Logger.Debug(table["value"]);
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading extension mappings!", ex);
            }
        }

        private static void LoadCustomExtensionMapping()
        {
            try
            {
                // get path of plugin configuration
                StringBuilder sbConfigFilePath = new StringBuilder(Win32.MAX_PATH);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbConfigFilePath);
                string configFilePath = sbConfigFilePath.ToString();

                string customExtensionMappingFilePath = ConfigFile.GetCustomExtensionMappingFilePath();

                StreamReader _textStreamReader;
                string customExtensionMappingJson;
                var serializer = new JavaScriptSerializer();

                _textStreamReader = File.OpenText(customExtensionMappingFilePath);
                customExtensionMappingJson = _textStreamReader.ReadToEnd();
                customExtensionMapping = serializer.Deserialize<Dictionary<string, string>>(customExtensionMappingJson);

                Logger.Debug("Loaded custom extension mapping");
            }
            catch (FileNotFoundException)
            {
                Logger.Debug("Custom extension mapping JSON file does not exist");
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading custom extension mappings!", ex);
            }
        }

        internal static void SetToolBarIcon()
        {
            // TODO: Maybe check out https://docs.microsoft.com/en-us/windows/desktop/controls/embed-nonbutton-controls-in-toolbars to create counter within toolbar as alternative?
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
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETSTATUSBAR, (IntPtr)NppMsg.STATUSBAR_DOC_TYPE, str);
            //Marshal.FreeHGlobal(pStr);
        }

        public static void OnNotification(ScNotification notification)
        {
            if (nppStarted && notification.Header.Code == (uint)SciMsg.SCN_MODIFIED)
            {
                Logger.Debug("[notification] SCN_MODIFIED, notification code: 0x" + notification.Header.Code.ToString("x"));
            }

            // TODO: NPPN_SHUTDOWN - save unpulsed to config (or do it in plugin unload sequence rather on bottom of this file)
            if (notification.Header.Code == (uint)NppMsg.NPPN_LANGCHANGED) // Does not seem to be triggered?
            {
                Logger.Debug("[notification] NPPN_LANGCHANGED");
                //currentLangDesc = GetCurrentLangDesc(); - below does this
                UpdateStatusbar();
            }
            if (notification.Header.Code == (uint)NppMsg.NPPN_READY) // IMPORTANT: It triggers SCN_MODIFIED for each opened file before this
            {
                Logger.Debug("[notification] NPPN_READY");
                nppStarted = true;
                UpdateStatusbar();
            }
            if (nppStarted && (notification.Header.Code == (uint)SciMsg.SCN_MODIFIED || notification.Header.Code == (uint)NppMsg.NPPN_TBMODIFICATION))
            {
                //Logger.Debug("[notification] SCN_MODIFIED #1");
                //UpdateStatusbar();
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
                if (Debug) Logger.Debug("[notification] SCN_CHARADDED");
                HandleActicity();
                if (Debug) Logger.Debug("SCN_CHARADDED - File: " + GetCurrentFile() + ", char: " + (char)notification.character + " (" + notification.character + "), lang: " + GetCurrentLanguage());
            }

            int SC_PERFORMED_USER_AND_SC_MOD_DELETETEXT = (int)SciMsg.SC_PERFORMED_USER | (int)SciMsg.SC_MOD_DELETETEXT;
            if (nppStarted && notification.Header.Code == (uint)SciMsg.SCN_MODIFIED && ((notification.ModificationType & SC_PERFORMED_USER_AND_SC_MOD_DELETETEXT) == SC_PERFORMED_USER_AND_SC_MOD_DELETETEXT))
            {
                Logger.Debug("[notification] SCN_MODIFIED #2, notification code: 0x" + notification.Header.Code.ToString("x"));
                // Looks like we can use this to track deleted stuff once Notepad++ is started and ready
                // It doesn't trigger on file close either, unlike on open with SC_MOD_INSERTTEXT, so we only use this, and SCN_CHARADDED for inserts
                // It will skip Ctrl+V if it wasn't pasted on some existing text, but never mind, it is still counting the most we want
                // And we would not like it to count random file opens or other actions in
                HandleActicity();
                if (Debug) Logger.Debug("SC_PERFORMED_USER & SC_MOD_DELETETEXT - File: " + GetCurrentFile() + ", char: " + notification.character + ", flags: " + notification.ModificationType.ToString("X"));
            }

            if (notification.Header.Code == (uint)SciMsg.SCEN_CHANGE) // Does not seem to be ever triggered (ah, right, GTK+ only it seems?)
            {
                if (Debug) Logger.Debug("SCEN_CHANGE - File: " + GetCurrentFile() + ", char: " + notification.character + ", lang: " + GetCurrentLanguage());
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
                if (Debug) Logger.Debug("Setting new statusbar text to: " + newStatusBarDocTypeText);
                SetStatusBarDocType(newStatusBarDocTypeText);
                if (Debug) Logger.Debug("New statusbar text was set.");
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

        private static void FlushCurrentPulseIfNeeded()
        {
            if (pulseQueue != null && currentPulse != null && !currentPulse.isEmpty())
            {
                pulseQueue.Enqueue(currentPulse);
                currentPulse = new Pulse();
                currentCount = 0;
            }
        }

        private static void ProcessPulses(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    if (pulseQueue != null && ((currentPulse != null && !currentPulse.isEmpty()) || !pulseQueue.IsEmpty) && EnoughTimePassed(DateTime.Now))
                    {
                        // if current pulse is not empty, add it to the queue now
                        FlushCurrentPulseIfNeeded();

                        // run only if the task didn't run yet, or the previous one already finished
                        if (pulseProcessor == null || pulseProcessor.IsCompleted || pulseProcessor.IsFaulted) // don't start if it's cancelled
                        {
                            try
                            {
                                pulseProcessor = ProcessPendingPulses(pulseProcessor_tokensource);
                            }
                            catch (OperationCanceledException) { }
                        }
                        //ProcessPulses();
                    }

                    UpdateStatusbar();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error processing pulses", ex);
                }
            });
        }

        private static Task ProcessPendingPulses(CancellationTokenSource tokenSource)
        {
            return Task.Run(async () =>
            {
                if (pulseQueue != null && ((currentPulse != null && !currentPulse.isEmpty()) || !pulseQueue.IsEmpty) && EnoughTimePassed(DateTime.Now))
                {
                    if (String.IsNullOrWhiteSpace(ApiKey))
                    {
                        Logger.Warning("No API token - cannot pulse!");
                        return;
                    }

                    if (proxyChangePending)
                    {
                        pulseProcessor_httpClientHandler = new HttpClientHandler();
                        pulseProcessor_httpClientHandler.Proxy = GetProxy();
                        pulseProcessor_client = new HttpClient(pulseProcessor_httpClientHandler);
                        proxyChangePending = false;
                    }
                    var jsonSerializer = new JavaScriptSerializer();

                    string URL;
                    bool usesCustomEndpoint = false;
                    if (String.IsNullOrWhiteSpace(ApiUrl))
                    {
                        URL = Constants.ApiEndpoint;
                    }
                    else
                    {
                        URL = ApiUrl;
                        usesCustomEndpoint = true;
                    }
                    if (!URL.EndsWith("my/pulses"))
                        URL += "my/pulses";

                    Pulse result;
                    while (pulseQueue.TryDequeue(out result))
                    {
                        if (!result.isEmpty())
                        {
                            bool error = false;
                            HttpResponseMessage response = null;
                            // Try to pulse to API
                            try
                            {
                                string json;
                                var httpRequestMessage = new HttpRequestMessage
                                {
                                    Method = HttpMethod.Post,
                                    RequestUri = new Uri(URL),
                                    Headers = {
                                        { "User-Agent", Constants.PluginUserAgent },
                                        { "Accept", "*/*" },
                                        { "X-API-Token", ApiKey }
                                    },
                                    // it will set Content-Type header for us
                                    Content = new StringContent(json = jsonSerializer.Serialize(result), Encoding.UTF8, "application/json")
                                };

                                Logger.Debug("Pulsing " + json);
                                response = await pulseProcessor_client.SendAsync(httpRequestMessage, tokenSource.Token);
                                response.EnsureSuccessStatusCode();
                                string JsonResult = await response.Content.ReadAsStringAsync();
                                _lastPulse = DateTime.Now;
                                if (!JsonResult.Contains(@"""ok""") && !JsonResult.Contains(@"success"))
                                {
                                    error = true;
                                    Logger.Error(@"Error pulsing, response does not contain ""ok"" or ""success"": " + JsonResult);
                                }
                                else
                                {
                                    Logger.Debug("Pulsed, response: " + JsonResult);
                                }
                            }
                            catch (TaskCanceledException)
                            {
                                pulseQueue.Enqueue(result); // requeue current pulse
                                return;
                            }
                            catch (HttpRequestException ex)
                            {
                                error = true;
                                if (response != null && response.StatusCode != 0)
                                {
                                    if ((int)response.StatusCode == 403)
                                    {
                                        Logger.Error("Could not pulse (error 403). Please make sure you entered a valid API token in Code::Stats settings.", ex);
                                        if (!_hasAlreadyShownInvalidApiTokenMessage) // we want to inform user only once, and if they do not provide the token, let's not bomb him with error each time after they type something
                                        {
                                            _hasAlreadyShownInvalidApiTokenMessage = true;
                                            MessageBox.Show("Could not pulse. Please make sure you entered a valid API token in Code::Stats settings.\nAll recorded XP from this session will be lost if you do not provide the correct API token!", "Code::Stats – error 403", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            PromptApiKey();
                                        }
                                    }
                                    else if ((int)response.StatusCode == 404 && usesCustomEndpoint)
                                    {
                                        Logger.Error("Could not pulse (error 404). The entered custom endpoint (" + URL + ") is invalid. ", ex);
                                        MessageBox.Show("Could not pulse. Invalid API endpoint URL. Please make sure you entered a valid API URL in Code::Stats settings or delete the value altogether to restore the default.\nAll recorded XP from this session will be lost if you do not provide the correct API URL path!", "Code::Stats – error 404", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                        SettingsPopup();
                                        _settingsForm.FocusTxtAPIURL();
                                        _settingsForm.ShowAPIURLTooltip();
                                    }
                                    else
                                    {
                                        Logger.Error("Could not pulse - HTTP error " + (int)response.StatusCode + ". Server response: " + response.Content.ReadAsStringAsync().Result, ex);
                                    }
                                }
                                else
                                {
                                    // response==null - no http status code available
                                    Logger.Error("Could not pulse. Are you behind a proxy? Try setting a proxy in Code::Stats settings with format https://user:pass@host:port. Exception Traceback", ex);
                                }
                            }
                            catch (Exception ex)
                            {
                                error = true;
                                Logger.Error("Error pulsing. Exception Traceback", ex);
                            }

                            if (error)
                            {
                                pulseQueue.Enqueue(result); // Requeue, since we failed to pulse
                                return;
                            }

                        }

                        tokenSource.Token.ThrowIfCancellationRequested();
                    }
                }
            }, tokenSource.Token);
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
            var foundDetectionMethod = Constants.DetectionType.EXTENSION_MAPPING;
            string language = "";

            foreach (var detectionMethod in DetectionOrder)
            {
                switch (detectionMethod)
                {
                    case Constants.DetectionType.CUSTOM_MAPPING:
                        if (!String.IsNullOrWhiteSpace(extension))
                        {
                            try
                            {
                                language = customExtensionMapping[extension];
                                found = true;
                            }
                            catch (KeyNotFoundException)
                            {
                                Logger.Debug("Extension \"" + extension + "\" is not found in custom extension mappings file.");
                                found = false;
                            }
                        }
                        break;
                    
                    case Constants.DetectionType.EXTENSION_MAPPING:
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
                                Logger.Debug("Extension \"" + extension + "\" is not found in extension mappings file.");
                                found = false;
                            }
                        }
                        break;

                    case Constants.DetectionType.LEXER_LANGUAGE:
                        found = true; // will set again to false if not
                        switch (langType)
                        {
                            case LangType.L_ADA: language = "Ada"; break;
                            //case LangType.L_ASCII: language = "ASCII"; break; // maybe Plain text?
                            case LangType.L_ASM: language = "Assembler"; break;
                            case LangType.L_ASP: language = "ASP"; break;
                            case LangType.L_AU3: language = "AutoIt"; break;
                            case LangType.L_BASH: language = "Shell Script"; break; // or Shell Script, probably called Shell in NPP (long desc: Unix script file)
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
                                found = false;
                                break;
                        }
                        break;
                }
                if (found)
                {
                    foundDetectionMethod = detectionMethod;
                    break;
                }
            }

            if (found && Debug)
                Logger.Debug("[GetCurrentLanguage] Detected language \"" + language + "\" with detection type: " + foundDetectionMethod);

            if (!found)
                language = "Plain text";

            return language;
        }

        public static bool EnoughTimePassed(DateTime now)
        {
            return _lastActivity < now.AddMilliseconds(-1 * pulseFrequency) && _lastPulse < now.AddMilliseconds(-1 * pulseFrequency);
        }

        public static void GetSettings(bool skipRead = false)
        {
            if (!skipRead) _CodeStatsConfigFile.Read();
            ApiKey = _CodeStatsConfigFile.ApiKey;
            ApiUrl = _CodeStatsConfigFile.ApiUrl;
            Debug = _CodeStatsConfigFile.Debug;
            Proxy = _CodeStatsConfigFile.Proxy;
            Stats = _CodeStatsConfigFile.Stats;
            CodeStatsPackage.Guid = _CodeStatsConfigFile.Guid;
            DetectionOrder = _CodeStatsConfigFile.DetectionOrder;

            proxyChangePending = true;
        }

        private static void PromptApiKey()
        {
            Logger.Info("Please input your API token into the Code::Stats window.");
            /*var form*/ _apikeyForm = new CodeStats.Forms.ApiKeyForm();
            _apikeyForm.ShowDialog();
        }

        private static void SettingsPopup()
        {
            if (_settingsForm == null)
            {
                _settingsForm = new CodeStats.Forms.SettingsForm();
                _settingsForm.OnConfigSaved += SettingsFormOnConfigSaved;
            }
            _settingsForm.Visible = false; // ?
            _settingsForm.ShowDialog();
        }

        private static void SettingsFormOnConfigSaved(object sender, EventArgs eventArgs)
        {
            GetSettings();
            LoadCustomExtensionMapping();
            _settingsForm.OnConfigSaved -= SettingsFormOnConfigSaved;
            _settingsForm.Dispose();
            _settingsForm = null;
        }

        public static void ReportStats()
        {
            Task.Run(() =>
            {
                try
                {
                    var client = new WebClient { Proxy = CodeStatsPackage.GetProxy() };
                    client.Headers[HttpRequestHeader.UserAgent] = Constants.PluginUserAgent;
                    string result = client.DownloadString("https://p0358.net/codestats/report.php?pluginver=" + Constants.PluginVersion
                        + "&cid=" + CodeStatsPackage.Guid + "&editorname=" + Constants.EditorName + "&editorver=" + Constants.EditorVersion
                        + "&is64process=" + ProcessorArchitectureHelper.Is64BitProcess.ToString().ToLowerInvariant() + "&is64sys=" + ProcessorArchitectureHelper.Is64BitOperatingSystem.ToString().ToLowerInvariant()
                        + "&osverstr=" + Constants.OSVersionString + "&osbuild=" + Constants.OSVersionBuild
                    ); // expected response: ok
                    if (result.Contains("ok")) _reportedStats = true;
                }
                finally { }
            });
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

            if (String.IsNullOrWhiteSpace(Proxy))
            {
                Logger.Debug("No proxy will be used. It's not set.");
                return proxy;
            }

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
            public static readonly Assembly Reference = typeof(CoreAssembly).Assembly;
            public static readonly Version Version = Reference.GetName().Version;
            // System.Reflection.Assembly.GetExecutingAssembly().Location
            public static readonly string Location = Reference.Location;
        }

        internal static void PluginCleanUp()
        {
            nppStarted = false;

            try
            {
                Logger.Info("Plugin cleanup on shutdown...");

                // Flush the current pulse
                Logger.Debug("Flushing the current pulse...");
                FlushCurrentPulseIfNeeded();

                Logger.Debug("Cancelling pulse processing...");
                pulseProcessor_tokensource.Cancel();

                if (timer != null)
                {
                    Logger.Debug("Stopping timer...");
                    timer.Stop();
                    timer.Elapsed -= ProcessPulses;
                    timer.Dispose();
                    timer = null;

                    // make sure the queue is empty
                    //ProcessPulses();
                }

                // test if we can cancel and dump pulses
                try
                {
                    if (pulseProcessor != null)
                    {
                        Logger.Debug("Waiting for pulse processor to be cancelled...");
                        pulseProcessor.Wait();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception while waiting for pulse processor to be cancelled", ex);
                }

                Logger.Debug("Dequeueing remaining queued pulses...");
                var jsonSerializer = new JavaScriptSerializer();
                Pulse result;
                while (pulseQueue.TryDequeue(out result))
                {
                    if (!result.isEmpty())
                    {
                        string json = jsonSerializer.Serialize(result);
                        Logger.Debug("Unsaved pulse: " + json);
                    }
                }

                Logger.Info("Plugin cleanup finished");
                Logger.FlushEverything();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception while performing plugin shutdown cleanup", ex);
                Logger.FlushEverything();
            }
        }
    }
}