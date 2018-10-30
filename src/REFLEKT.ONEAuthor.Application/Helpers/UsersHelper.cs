using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Models;
using System;
using System.IO;

namespace REFLEKT.ONEAuthor.Application.Helpers
{
    public static class UsersHelper
    {
        private static string tempToken = "qwerty123456";
        public static string tempUser = "local";
        private static string tempPass = "pass";

        public static string GetUserFolder()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = path.Replace("\\AppData", "\\RFOneAuthor");
            }
            path = path.Replace("\\", "/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static bool CheckUserDraft(string token)
        {
            string path = Path.Combine(GetUserFolder(), "admins");
            if (!Directory.Exists(path))
                return false;

            foreach (string s in Directory.GetFiles(path))
            {
                if (Path.GetExtension(s).Contains("json"))
                {
                    UserModel model = JsonConvert.DeserializeObject<UserModel>(File.ReadAllText(s));
                    if (model.StaticTicket == token)
                    {
                        return model.Draft;
                    }
                }
            }
            return false;
        }

        public static UserModel GetRandomUser()
        {
            string path = Path.Combine(GetUserFolder(), "admins");
            if (!Directory.Exists(path))
                return null;

            foreach (string s in Directory.GetFiles(path))
            {
                if (Path.GetExtension(s).Contains("json"))
                {
                    UserModel model = JsonConvert.DeserializeObject<UserModel>(File.ReadAllText(s));
                    return model;
                }
            }
            return null;
        }

        public static string GetServer(string login)
        {
            string path = Path.Combine(GetUserFolder(), "admins");
            string filePath = Path.Combine(path, login + ".json");
            if (!Directory.Exists(path) || !File.Exists(filePath))
                return string.Empty;

            UserModel model = JsonConvert.DeserializeObject<UserModel>(File.ReadAllText(filePath));
            if (string.IsNullOrEmpty(model.Server))
            {
                return "localhost";
            }
            return model.Server;
        }

        public static string GetToolsFolder()
        {
            string pathToConfig = Path.Combine(PathHelper.GetUserProcessingFolder(), "repo.config");
            if (!File.Exists(pathToConfig))
            {
                return "localhost";
            }

            RepoModel model = JsonConvert.DeserializeObject<RepoModel>(File.ReadAllText(pathToConfig));
            return model.FolderName;
        }

        public static void SetRepoPath(string repo)
        {
            string pathToConfig = Path.Combine(PathHelper.GetUserProcessingFolder(), "repo.config");

            RepoModel model = !File.Exists(pathToConfig)
                ? new RepoModel()
                : JsonConvert.DeserializeObject<RepoModel>(File.ReadAllText(pathToConfig));

            model.FolderName = repo;

            if (!Directory.Exists(Path.GetDirectoryName(pathToConfig)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(pathToConfig));
            }

            File.WriteAllText(pathToConfig, JsonConvert.SerializeObject(model));
        }

        public static string GetVersion()
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            string[] split = version.Split(new char[] { '.' });

            if (split.Length >= 3)
            {
                version = string.Format("{0}.{1}.{2}", split[0], split[1], split[2]);
            }
            return version;
        }
    }
}