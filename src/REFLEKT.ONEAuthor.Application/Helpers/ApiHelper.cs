using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Models;
using REFLEKT.ONEAuthor.Application.Utilities;
using CategoryModel = REFLEKT.ONEAuthor.Application.Models.CategoryModel;

namespace REFLEKT.ONEAuthor.Application.Helpers
{
    public class ApiHelper
    {
        public static List<string> JsonError(int statusCode, string[] errors)
        {
            List<string> res = new List<string>();

            foreach (string e in errors)
            {
                res.Add(e);
            }

            return res;
        }

        public static string GetCurrentDate()
        {
            string res = "";

            DateTime now = DateTime.Now;

            if (now.Day > 9)
                res += now.Day.ToString();
            else
                res += "0" + now.Day.ToString();

            res += ".";

            if (now.Month > 9)
                res += now.Month.ToString();
            else
                res += "0" + now.Month.ToString();

            res += "." + now.Year + "_";

            if (now.Hour > 9)
                res += now.Hour.ToString();
            else
                res += "0" + now.Hour.ToString();

            res += ".";

            if (now.Minute > 9)
                res += now.Minute.ToString();
            else
                res += "0" + now.Minute.ToString();
            return res;
        }

        public static string FindFolderById(string id, string root)
        {
            string[] idSplit = id.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            id = idSplit[idSplit.Length - 1].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];

            foreach (string folder in Directory.GetDirectories(root))
            {
                if (File.Exists(Path.Combine(folder, new DirectoryInfo(folder).Name + ".info")))
                {
                    string folderName = new DirectoryInfo(folder).Name;
                    string[] nameSplit = folderName.Split('.');

                    if (nameSplit[0] == id || folderName == id)
                        return Path.Combine(root, folderName);

                    string ret = FindFolderById(id, Path.Combine(root, folderName));

                    if (!string.IsNullOrEmpty(ret))
                        return ret;
                }
            }

            return null;
        }

        public static byte[] DecodeBase64(string str)
        {
            string dummyData = str.Trim().Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
                dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');

            byte[] converted = Convert.FromBase64String(dummyData);
            return converted;
            byte[] decbuff = Convert.FromBase64String(str);
            return decbuff;
        }

        /*public static string GetScenarioInfo(string id, string path)
        {
            string folder = new DirectoryInfo(path).Name;
            //string topicsPath = Path.Combine(path, "topics");
            DescModel scenario = JsonConvert.DeserializeObject<DescModel>(File.ReadAllText(Path.Combine(path, folder + ".info")));
            ScenarioObject sc = new ScenarioObject();
            sc.id = id;
            sc.name = scenario.title;
            sc.topics = GetTopics(id);
            sc.description = scenario.desc;// Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(scenario.desc));
            string json = "{ scenario:" + JsonConvert.SerializeObject(sc) + "}";
            return json;
        }*/

        public static ScenarioModel GetScenarioInfoModel(string id, string path)
        {
            string folder = new DirectoryInfo(path).Name;
            //string topicsPath = Path.Combine(path, "topics");
            DescModel scenario =
                JsonConvert.DeserializeObject<DescModel>(File.ReadAllText(Path.Combine(path, folder + ".info")));
            ScenarioObject sc = new ScenarioObject();
            sc.id = id;
            sc.name = scenario.title;
            sc.topics = GetTopics(id);
            sc.description =
                scenario.desc; // Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(scenario.desc));
            /*foreach (string topic in Directory.GetFiles(topicsPath))
            {
                sc.topics.Add(new FileInfo(topic).Name);
            }*/

            ScenarioModel model = new ScenarioModel();
            model.scenario = sc;
            return model;
        }

        public static FolderModel GetFolderInfo(string id, string path)
        {
            string folder = new DirectoryInfo(path).Name;
            CategoryModel category =
                JsonConvert.DeserializeObject<CategoryModel>(File.ReadAllText(Path.Combine(path, folder + ".info")));
            FolderObject fl = new FolderObject();
            fl.id = id;
            fl.name = category.title;
            string parentName = new DirectoryInfo(path).Parent.Name;
            if (FindFolderById(parentName.Split('.')[0], PathHelper.GetRepoPath()) == null)
            {
                fl.type = FolderType.Category.ToString();
                fl.parent = null;
            }
            else
            {
                fl.type = FolderType.Product.ToString();
                fl.parent = parentName;
            }

            FolderModel model = new FolderModel();
            model.folder = fl;
            //string json = "{ folder:\"" + JsonConvert.SerializeObject(fl) + "\"}";
            return model;
        }

        public static ScenarioModel CreateScenario(string folder_id, string name, string description)
        {
            string folder = ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath());
            string ret = Command.CreateScenario(name, folder, description);
            folder = ApiHelper.FindFolderById(ret, PathHelper.GetRepoPath());
            return ApiHelper.GetScenarioInfoModel(ret, folder);
        }

        public static ScenarioModel CreateScenarioModel(string folder_id, string name, string description)
        {
            string folder = ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath());
            string ret = Command.CreateScenario(name, folder, description);
            folder = ApiHelper.FindFolderById(ret, PathHelper.GetRepoPath());
            ScenarioModel rett = ApiHelper.GetScenarioInfoModel(ret, folder);
            return rett;
        }

        public static FolderModel CreateFolder(string folder_id, string name)
        {
            string folder = folder_id != ""
                ? ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath())
                : PathHelper.GetRepoPath();
            string id = Command.CreateCategory(name, folder);
            folder = ApiHelper.FindFolderById(id, PathHelper.GetRepoPath());
            return ApiHelper.GetFolderInfo(id, folder);
            //return ret;
        }

        /// <summary>
        /// Retrieves data from *.info files within ~\topics folder for particular scenario
        /// </summary>
        /// <param name="scenarioId">ID of scenario to retireve topics from</param>
        /// <returns>Deserialized object from *.info file</returns>
        public static List<TopicObject> GetTopics(string scenarioId)
        {
            var scenarioRootFolder = FindFolderById(scenarioId, PathHelper.GetRepoPath());
            EnsureScenarioFoldersExist(scenarioRootFolder);

            var topicsDirectory = Path.Combine(scenarioRootFolder, "topics");
            var serializer = new JsonSerializerUtility();

            var result = Directory.GetFiles(topicsDirectory)
                .Select(topicFile => serializer.DeserializeFromFile<TopicModel>(topicFile))
                .Select(topic => topic.ConvertToDto()).ToList();

            return result;
        }

        public static List<TopicModel> GetTopicsOrigin(string scenario_id)
        {
            string scenarioRootFolder = FindFolderById(scenario_id, PathHelper.GetRepoPath());
            EnsureScenarioFoldersExist(scenarioRootFolder);

            var topicsDirectory = Path.Combine(scenarioRootFolder, "topics");
            var objects = new List<TopicModel>();
            foreach (string t in Directory.GetFiles(topicsDirectory))
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(t));
                OldTopicModel oldTopic = JsonConvert.DeserializeObject<OldTopicModel>(File.ReadAllText(t));
                if (!string.IsNullOrEmpty(oldTopic.pathToZip))
                {
                    topic.localization = "default";
                    topic.pathToZip = oldTopic.pathToZip;
                }

                if (!string.IsNullOrEmpty(oldTopic.vmpPath))
                {
                    topic.localization = "default";
                    topic.vmpPath = oldTopic.vmpPath;
                }

                if (string.IsNullOrEmpty(topic.pathToZip) && !string.IsNullOrEmpty(topic.pathToZipDEFAULT))
                {
                    topic.pathToZip = topic.pathToZipDEFAULT;
                    File.WriteAllText(t, JsonConvert.SerializeObject(topic));
                }

                topic.infoPath = t;
                objects.Add(topic);
            }

            return objects;
        }

        public static void EnsureScenarioFoldersExist(string scenarioRootFolder)
        {
            if (Directory.Exists(scenarioRootFolder) &&
                Directory.Exists(Path.Combine(scenarioRootFolder, "topics"))) // legacy, need redesign 
            {
                return;
            }

            Directory.CreateDirectory(Path.Combine(scenarioRootFolder, "topics"));
            Directory.CreateDirectory(Path.Combine(scenarioRootFolder, "default"));
            Directory.CreateDirectory(Path.Combine(scenarioRootFolder, "default/pub_in"));
            Directory.CreateDirectory(Path.Combine(scenarioRootFolder, "default/pub_out"));
            Directory.CreateDirectory(Path.Combine(scenarioRootFolder, "default/pub_draft"));
            Directory.CreateDirectory(Path.Combine(scenarioRootFolder, "default/pub_offline"));
            Directory.CreateDirectory(Path.Combine(scenarioRootFolder, "default/projects"));
        }
    }
}