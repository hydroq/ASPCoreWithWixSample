using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace REFLEKT.ONEAuthor.Executor
{
    public static class Program
    {
        private const string ServerBatchName = "xsp4.bat";
        private const string ServerBaseDirectory = "webview.local";

        private const string ClientExecutableName = "REFLEKT ONE Author.exe";
        private const string CilentBaseDirectory = "REFLEKT ONE Author-win32-x64";

        private const int SuccessExitCode = 0;

        private static readonly string ApplicationRootDirectory = Environment.CurrentDirectory;

        public static void Main(string[] args)
        {
            try
            {
                StartServer();

                RunViewer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }

            Environment.Exit(0);
        }

        private static void StartServer()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C cd " + Path.Combine(ApplicationRootDirectory, ServerBaseDirectory) + " & " + ServerBatchName,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                WorkingDirectory = ApplicationRootDirectory
            };

            Process.Start(processStartInfo);
        }

        private static void RunViewer()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(ApplicationRootDirectory, CilentBaseDirectory, ClientExecutableName),
                WindowStyle = ProcessWindowStyle.Maximized,
                UseShellExecute = false
            };

            Process.Start(processStartInfo);
        }

        private static void EnsureProcessRanWithNoErrors(ProcessStartInfo targetProcessInfo)
        {
            var targetProcess = Process.Start(targetProcessInfo);

            targetProcess?.WaitForExit();
            if (targetProcess?.ExitCode != SuccessExitCode)
            {
                throw new InvalidOperationException(targetProcess?.StandardError.ReadToEnd());
            }
        }
    }
}