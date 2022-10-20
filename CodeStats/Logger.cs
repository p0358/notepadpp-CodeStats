using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        internal static bool hasAlreadyShownErrorBox = false;
        private static StreamWriter writer;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static ConcurrentQueue<Func<Task>> funcTaskQueue = new ConcurrentQueue<Func<Task>>();

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
            var exceptionMessage = $"{msg}: {ex}";

            Log(LogLevel.HandledException, exceptionMessage);
        }

        private static bool RunNextLogTask()
        {
            Func<Task> result;
            // first only peek without removing, so that EnqueueLogTask won't call another out-of-chain RunNextLogTask
            if (funcTaskQueue.TryPeek(out result))
            {
                Task.Run(result).ContinueWith(task => {
                    // dequeue current log line only now
                    funcTaskQueue.TryDequeue(out result);
                    RunNextLogTask();
                });
                return true;
            }
            return false; // nothing remained
        }

        private static void EnqueueLogTask(Func<Task> taskFunc)
        {
            bool wasEmpty = funcTaskQueue.IsEmpty;
            funcTaskQueue.Enqueue(taskFunc);
            if (wasEmpty)
                RunNextLogTask();
        }

        internal static void Log(LogLevel level, string msg)
        {
            EnqueueLogTask(async () =>
            {
                try
                {
                    await semaphore.WaitAsync();

                    if (writer == null) writer = Setup(); // we'll try to keep the file opened all the time and see how bad we end up with it
                    if (writer == null) return;

                    await writer.WriteLineAsync(String.Format("[Code::Stats {0} {1}] {2}", Enum.GetName(level.GetType(), level), DateTime.Now.ToString("HH:mm:ss"), msg));
                    await writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    if (!hasAlreadyShownErrorBox)
                    {
                        hasAlreadyShownErrorBox = true;
                        MessageBox.Show(ex.ToString() + "\n\nNo further log writing errors will be shown in this session to avoid interrupting your work.", "Error writing to log file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //MessageBox.Show(string.Format("{0}\\{1}.log", configDir, Constants.PluginName), "Error writing to log file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }

        // called on shutdown to ensure everything is flushed synchronously
        public static void FlushEverything()
        {
            if (writer == null)
                return;

            // let all log lines be written first
            /*while (true)
            {
                if (!RunNextLogTask())
                    break;
            }*/
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!funcTaskQueue.IsEmpty && stopwatch.ElapsedMilliseconds < 1000) // wait for max 1 second
            {
                semaphore.Wait();
                semaphore.Release();
            }
            stopwatch.Stop();

            semaphore.Wait();
            writer.Flush();
            semaphore.Release();
        }

        private static StreamWriter Setup()
        {
            if (String.IsNullOrWhiteSpace(configDir))
            {
                // get path of plugin configuration
                StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
                configDir = sbIniFilePath.ToString();
            }
            if (string.IsNullOrWhiteSpace(configDir)) return null;

            var filename = string.Format("{0}\\{1}.log", configDir, "CodeStats");
            var writer = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.Read));
            return writer;
        }

        public static void Delete()
        {
            if (String.IsNullOrWhiteSpace(configDir))
            {
                // get path of plugin configuration
                StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
                configDir = sbIniFilePath.ToString();
            }
            if (string.IsNullOrWhiteSpace(configDir)) return;

            var filename = string.Format("{0}\\{1}.log", configDir, "CodeStats");
            File.Delete(filename);
        }
    }
}
