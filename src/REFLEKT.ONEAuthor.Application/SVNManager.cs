using REFLEKT.ONEAuthor.Application.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace REFLEKT.ONEAuthor.Application
{
    public class SVNManager
    {
        public static void SaveChanges()
        {
        }

        public static void Authorizate(string server, string user, string password, Action<string> onFinished)
        {
            string dataPath = UsersHelper.GetUserFolder();

            if (server != "localhost")
            {
                string[] serverSplits = server.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                if (serverSplits.Length == 0)
                    return;

                string projectFolder = serverSplits[serverSplits.Length - 1];
                UsersHelper.SetRepoPath(projectFolder);

                if (Directory.Exists(PathHelper.GetRepoPath()))
                {
                    onFinished("checked");
                    return;
                }

                string cmd = string.Format("cd \"" + UsersHelper.GetUserFolder() + "\" & svn co --username {0} --password {1} --trust-server-cert --non-interactive {2} {3}", user, password, server, projectFolder);
                Execute(cmd, onFinished);
            }
            else
            {
                UsersHelper.SetRepoPath("localhost");
                if (Directory.Exists(PathHelper.GetRepoPath()))
                {
                    onFinished("checked");
                    return;
                }

                string cmd = "svnadmin create \"" + Path.Combine(dataPath, "localhost").Replace("\\", "/") + "\"";
                Execute(cmd, (res) =>
                {
                    string importCmd = "svn import \"" + dataPath.Replace("\\", "/") + "/localhost\" \"file://" + dataPath.Replace("\\", "/") + "/localhost\" -m \"Init\"";

                    dataPath = dataPath.Replace("\\", "/");
                    importCmd = "svn import \"" + dataPath.Replace("\\", "/") + "/localhost\" \"file:///" + dataPath.Replace("\\", "/") + "/localhost\" -m \"Init\"";
                    Execute(importCmd, (ress) =>
                    {
                        string cocmd = "svn co \"file:///" + dataPath + "/localhost\" \"" + dataPath + "/localhost\"";
                        cocmd = "svn co \"file:///" + dataPath.Replace("\\", "/") + "/localhost\" \"" + Path.Combine(dataPath, "localhost").Replace("\\", "/") + "\"";
                        Execute(cocmd, onFinished);
                        onFinished?.Invoke("checked");
                    });
                });
            }
        }

        public static bool IsLocal()
        {
            return UsersHelper.GetToolsFolder() == "localhost";
        }

        public static void GetCurrentRevision(Action<string> onResult = null)
        {
            Execute("svn info | grep \"Revision\" | awk '{print $2}'", onResult);
        }

        public static void GetCurrentRevisionFile(string file, Action<string> onResult = null)
        {
            //Execute("svn info \""+file+"\" | grep \"Revision\" | awk '{print $2}'", onResult);
            Execute("svn info \"" + file + "\" -r HEAD --trust-server-cert --non-interactive| findstr Revision", onResult);
        }

        public static void GetLastRevision(Action<string> onResult)
        {
            Execute("svn info -r HEAD --trust-server-cert --non-interactive| findstr Revision", onResult);
        }

        public static void RevertToRevision(int revision, string file = "", Action<string> onResult = null)
        {
            Execute("svn merge -r HEAD:" + revision + " \"" + file + "\" --accept theirs-full --trust-server-cert --non-interactive", onResult);
        }

        public static void RevertToLastVersion(string file = "", Action<string> onResult = null)
        {
            Execute("svn revert -R \"" + file + "\" --trust-server-cert --non-interactive", onResult);
        }

        public static void GetInfo(Action<string> onResult = null)
        {
            Execute("svn info --trust-server-cert --non-interactive", onResult);
        }

        public static void GetLastChanges(Action<string> onResult = null)
        {
            Execute("svn status", onResult);
        }

        public static void GetChanges(string file, Action<string> onResult = null)
        {
            Execute("svn blame -v " + file, onResult);
        }

        public static void GetChangesInRevision(int revision, Action<string> onResult = null)
        {
            Execute("svn diff -c " + revision, onResult);
        }

        public static void GetChangesInRevisionByCurrent(int revision, Action<string> onResult = null)
        {
            Execute("svn diff -r " + revision, onResult);
        }

        public static void GetAllRevisionsForFile(string file, Action<string> onResult = null)
        {
            Execute("svn log \"" + file + "\" --trust-server-cert --non-interactive", onResult);
        }

        public static void GetFileInRevision(int revision, string file, Action<string> onResult = null)
        {
            Execute("svn cat -r " + revision + " " + file + " --trust-server-cert --non-interactive", onResult);
        }

        public static void AddFile(string file, Action<string> onResult = null)
        {
            Execute("svn add " + file, onResult);
        }

        public static void DeleteFile(string file, Action<string> onResult = null)
        {
            Execute("svn delete \"" + file + "\"", onResult);
        }

        public static void Rename(string oldFile, string newFile, Action<string> onResult = null)
        {
            Execute("svn mv \"" + oldFile + "\" \"" + newFile + "\"", onResult);
        }

        public static void Changelist(string file, Action<string> onResult = null)
        {
            Execute("svn add --force ./* ", onResult);
        }

        public static void RemoveMissingFiles(Action<string> onResult = null)
        {
            Execute("svn status | findstr /R \"^!\" > ../missing.list", (res) =>
            {
                string rfonePath = PathHelper.GetRepoPath();
                if (!rfonePath.EndsWith("\\") && !rfonePath.EndsWith("/"))
                {
                    rfonePath += "\\";
                }
                string result = "";
                string[] files = File.ReadAllLines(Path.Combine(UsersHelper.GetUserFolder(), "missing.list"));
                foreach (string f in files)
                {
                    string file = f.Replace("!       ", rfonePath);
                    bool deleted = false;
                    DeleteFile(file, (ress) =>
                    {
                        result += ress + "\r\n";
                        deleted = true;
                    });
                    while (!deleted) { Thread.Sleep(100); }
                }

                Commit("delete", (resss) =>
                {
                    UpdateLocalRepo((ressss) =>
                    {
                        onResult(string.Empty);
                    });
                });
            });
        }

        public static void GetModifiedFilesFromServer(Action<string> onResult = null)
        {
            Execute("svn merge --dry-run -r BASE:HEAD .", onResult);
        }

        public static void LockFile(string file, Action<string> onResult = null)
        {
            Execute("svn lock " + file, onResult);
        }

        public static void UnlockFile(string file, Action<string> onResult = null)
        {
            Execute("svn unlock " + file, onResult);
        }

        public static void CleanUp()
        {
            Execute("svn cleanup", (res) =>
            {
                ResolveConflicts();
            });
        }

        public static void Commit(string comment, Action<string> onResult = null)
        {
            Execute("svn commit -m \"" + comment + "\"", onResult);
        }

        public static void ResolveConflicts(Action<string> onResult = null)
        {
            Execute("svn resolve -R --accept mine-full", onResult);
        }

        public static void UpdateLocalRepo(Action<string> onResult)
        {
            Execute("svn update", onResult);
        }

        public static void UpdateLocalRepo()
        {
            Execute("svn update");
        }

        private static void Execute(string command, Action<string> onResult = null)
        {
            string repoPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());
            command = "/C cd /d \"" + repoPath + "\" & " + command;

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            Process process = Process.Start(processInfo);
            string result = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            process.WaitForExit();
            process.Close();

            onResult?.Invoke(result + " " + errors);
        }
    }
}