using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace REFLEKT.ONEAuthor.Application.Helpers
{
    public class ExternalProgram
    {
        #region DLLImports

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern System.IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("User32.dll")]
        private static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr parentWindow, IntPtr previousChildWindow, string windowClass, string windowTitle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr window, out int process);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SWP_NOOWNERZORDER = 0x200;
        private const int SWP_NOREDRAW = 0x8;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int WS_EX_MDICHILD = 0x40;
        private const int SWP_FRAMECHANGED = 0x20;
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_ASYNCWINDOWPOS = 0x4000;

        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 0x10000000;
        private const int WM_CLOSE = 0x10;
        private const int WS_CHILD = 0x40000000;

        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x1;
        private const UInt32 SWP_NOMOVE = 0x2;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        #endregion DLLImports

        private Process appProcess;
        private static readonly ILogger _logger = ApplicationLogging.LoggerFactory.CreateLogger<ExternalProgram>();

        public static void OpenAppWindow(TopicModel topic,
                                        string topicPath,
                                        string appName,
                                        string webRootPath,
                                        Action onStart = null,
                                        Action<TopicModel, string, string> onExit = null)
        {
            Setup(topic.vmpPath, topicPath, appName, webRootPath, topic, onStart, onExit);
        }

        private static void Setup(string pathToVmp, 
                                string topicPath, 
                                string appName, 
                                string webRootPath,
                                TopicModel topic, 
                                Action onStart, 
                                Action<TopicModel, string, string> onExit)
        {
            Show(topicPath, pathToVmp, appName, webRootPath,
                () =>
                    {
                        _logger.LogDebug("Starting Cortona, locking files in SVN");
                        SVNManager.LockFile(pathToVmp.Replace(PathHelper.GetRepoPath(), ""));
                        onStart?.Invoke();
                    },
                (sender, e) =>
                    {
                        _logger.LogDebug("Exit Cortona");
                        SVNManager.UnlockFile(pathToVmp.Replace(PathHelper.GetRepoPath(), ""));

                        onExit(topic, topicPath, pathToVmp);
                    });
        }

        private static void Show(string topicPath, string vnp, string appName, string webRootPath, Action onAppStart, Action<object, EventArgs> onAppExit)
        {
            string cortonaCachePath = "C:/ProgramData/ParallelGraphics/VM/TC_Cache";
            if (Directory.Exists(cortonaCachePath))
            {
                try
                {
                    PathHelper.ForceDeleteFolderContentRecursively(cortonaCachePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error cleaning folder {cortonaCachePath}");
                }
            }

            ExternalProgram external = new ExternalProgram();

            TopicModel model = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(topicPath));
            OldTopicModel oldModel = JsonConvert.DeserializeObject<OldTopicModel>(File.ReadAllText(topicPath));

            if (!string.IsNullOrEmpty(oldModel.pathToZip))
            {
                model.localization = "default";
                model.pathToZip = oldModel.pathToZip;
            }
            if (!string.IsNullOrEmpty(oldModel.vmpPath))
            {
                model.localization = "default";
                model.vmpPath = oldModel.vmpPath;
            }

            if (!File.Exists(appName))
            {
                _logger.LogError($"Error opening Cortona, file doesn't exist: {appName}");
                return;
            }

            external.ShowApp(appName, vnp, webRootPath, onAppExit);

            model.isOpened = 1;
            onAppStart();

            File.WriteAllText(topicPath, JsonConvert.SerializeObject(model));
        }

        public void ShowApp(string program, string vnp, string webRootPath, Action<object, EventArgs> onAppExit)
        {
            string projectsDir = vnp.Replace(Path.GetFileName(vnp), "");
            string command = "/CMS PG.CMSTC /EXCHANGEPATH \"" + projectsDir.Remove(projectsDir.Length - 1) + "\" /IN " + Path.GetFileNameWithoutExtension(vnp);

            string path = Path.Combine(PathHelper.GetUserProcessingFolder(), "cortona.bat");
            string content = string.Format("\"{0}\" {1}", program, command);
            File.WriteAllText(path, content);

            ProcessStartInfo info = new ProcessStartInfo(path, command)
            {
                WindowStyle = ProcessWindowStyle.Minimized,
                WorkingDirectory = Environment.CurrentDirectory
            };

            appProcess = Process.Start(info);
            if (appProcess == null)
            {
                throw new InvalidOperationException("Cannot start cortona");
            }

            appProcess.EnableRaisingEvents = true;
            appProcess.Exited += new EventHandler(onAppExit);

            WindowHelper.BringProcessToFront(appProcess);
        }
    }

    public static class WindowHelper
    {
        private static ILogger _logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(WindowHelper));

        public static void BringProcessToFront(Process process)
        {
            try
            {
                IntPtr handle = process.MainWindowHandle;
                ShowWindow(handle, SW_RESTORE);
                SetForegroundWindow(handle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bringing process to front");
            }
        }

        private const int SW_RESTORE = 9;

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);
    }
}