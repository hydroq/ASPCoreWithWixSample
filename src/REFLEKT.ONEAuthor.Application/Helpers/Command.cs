using System;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using REFLEKT.ONEAuthor.Application.Models;

namespace REFLEKT.ONEAuthor.Application.Helpers
{
    public class Command
    {
        private static readonly ILogger Logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(Command));

        public static string CreateCategory(string name, string path)
        {
            CategoryModel model = new CategoryModel();
            model.title = name;
            return CreateFolder(name, path, JsonConvert.SerializeObject(model), false);
        }

        public static string CreateProduct(string name, string path)
        {
            CategoryModel model = new CategoryModel();
            model.title = name;
            return CreateFolder(name, path, JsonConvert.SerializeObject(model), false);
        }

        public static string CreateScenario(string name, string path, string desc = "")
        {
            DescModel model = new DescModel();
            model.title = name;
            model.desc = desc;
            return CreateFolder(name, path, JsonConvert.SerializeObject(model), true);
        }

        public static string CreateFolder(string name, string path, string json, bool isScenario)
        {
            if (Directory.Exists(path))
            {
                string id = PathHelper.GenerateId(10);
                name = id;
                path = Path.Combine(path, name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    string filename = Path.Combine(path, name + ".info");
                    File.WriteAllText(filename, json);
                    if (isScenario)
                    {
                        Directory.CreateDirectory(Path.Combine(path, "default"));
                        Directory.CreateDirectory(Path.Combine(path, "default", "projects"));
                        Directory.CreateDirectory(Path.Combine(path, "default", "pub_in"));
                        Directory.CreateDirectory(Path.Combine(path, "default", "pub_out"));
                        Directory.CreateDirectory(Path.Combine(path, "default", "pub_draft"));
                        Directory.CreateDirectory(Path.Combine(path, "topics"));
                    }
                    return id;
                }
            }
            return null;
        }

        public static bool RemoveFolder(string path, short tryCount = 3)
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException e)
            {
                if (tryCount <= 1)
                {
                    Logger.LogError(e, $"Error deleting folder {path}, giving up");

                    throw;
                }

                Logger.LogError(e, $"Error deleting folder {path}, retrying in 5 sec...");
                Thread.Sleep(5000);
                RemoveFolder(path, --tryCount);
            }

            return true;
        }

        private void RemoveTopic(string path, string topic)
        {
            string info = Path.Combine(path, "topics", topic + ".info");
            if (File.Exists(info))
            {
                TopicModel model = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(info));
                string[] localizations = new string[] { "default", "en_en", "ru_ru" };
                foreach (string loc in localizations)
                {
                    model.localization = loc;
                    if (!string.IsNullOrEmpty(model.pathToZip))
                    {
                        File.Delete(model.pathToZip);
                    }
                    if (!string.IsNullOrEmpty(model.vmpPath))
                    {
                        File.Delete(model.vmpPath);
                        File.Delete(model.vmpPath.Replace(".vmp", ".vmb"));
                    }
                }
                File.Delete(info);
            }
        }

        private void EditInfo(string path, string newName)
        {
            string pathToInfo = Path.Combine(path, new DirectoryInfo(path).Name + ".info");
            if (File.Exists(pathToInfo))
            {
                CategoryModel model = JsonConvert.DeserializeObject<CategoryModel>(File.ReadAllText(pathToInfo));
                model.title = newName;
                File.WriteAllText(pathToInfo, JsonConvert.SerializeObject(model));
            }
        }

        private void EditInfo(string path, string newName, string newDesc)
        {
            string pathToInfo = Path.Combine(path, new DirectoryInfo(path).Name + ".info");
            if (File.Exists(pathToInfo))
            {
                DescModel model = JsonConvert.DeserializeObject<DescModel>(File.ReadAllText(pathToInfo));
                if (!string.IsNullOrEmpty(newName))
                    model.title = newName;
                if (!string.IsNullOrEmpty(newDesc))
                    model.desc = newDesc;
                File.WriteAllText(pathToInfo, JsonConvert.SerializeObject(model));
            }
        }

        private void EditZipPath(string topic, string path)
        {
            if (File.Exists(topic))
            {
                TopicModel model = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(topic));
                model.localization = "default";
                model.pathToZip = path;
                File.WriteAllText(topic, JsonConvert.SerializeObject(model));
            }
        }

        private void EditTopicIndex(string path, string topic, int to)
        {
            topic = Path.Combine(path, topic + ".info").Replace('\\', '/');

            if (File.Exists(topic))
            {
                TopicModel baseModel = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(topic));
                int from = baseModel.index;
                if (from == to)
                    return;
                baseModel.index = to;
                foreach (string file in Directory.GetFiles(path))
                {
                    if (file.Replace('\\', '/') != topic)
                    {
                        TopicModel model = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(file));
                        if (model.index > from && model.index <= to && from < to)
                        {
                            model.index--;
                            File.WriteAllText(file, JsonConvert.SerializeObject(model));
                        }
                        else if (model.index < from && model.index >= to && from > to)
                        {
                            model.index++;
                            File.WriteAllText(file, JsonConvert.SerializeObject(model));
                        }
                    }
                }
                File.WriteAllText(topic, JsonConvert.SerializeObject(baseModel));
            }
        }
    }
}