using System.IO;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using IOFile = System.IO.File;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class CommandController : Controller
    {
        [HttpGet]
        [Route("Command")]
        public IActionResult Index()
        {
            string projectPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());
            string command = Request.Query["command"];
            string path = HttpUtility.UrlDecode(Request.Query["path"]);

            if (!string.IsNullOrEmpty(command) && !string.IsNullOrEmpty(path))
            {

                string[] folders = path.Split('/', '\\');

                if (command == "create")
                {
                    if (folders.Length == 1)
                    {
                        CreateCategory(folders[0], projectPath);
                    }
                    else if (folders.Length == 2)
                    {
                        CreateProduct(folders[1], Path.Combine(projectPath, folders[0]));
                    }
                    else if (folders.Length == 3)
                    {

                        CreateScenario(folders[2], Path.Combine(projectPath, folders[0], folders[1]));
                    }
                }
                else if (command == "remove")
                {
                    if (folders.Length > 0 && folders.Length <= 3)
                    {
                        RemoveFolder(Path.Combine(projectPath, folders[0]));
                    }
                    else if (folders.Length == 4)
                    {
                        RemoveTopic(Path.Combine(projectPath, folders[0], folders[1], folders[2]), folders[3]);
                    }
                }
                else if (command == "draft")
                {
                    if (folders.Length == 3)
                    {
                        Publishing.Draft(Path.Combine(projectPath, path), (res) => { Content("result: " + res); });
                    }
                }
                else if (command == "publish")
                {
                    if (folders.Length == 3)
                    {
                        Publishing.Publish(Path.Combine(projectPath, path), (res) => { });

                        return Ok();
                    }
                }
                else if (command == "edit")
                {
                    if (folders.Length > 0 && folders.Length <= 2)
                    {
                        string newName = Request.Query["name"];
                        if (!string.IsNullOrEmpty(newName))
                            EditInfo(Path.Combine(projectPath, path), newName);
                    }
                    else if (folders.Length == 3)
                    {
                        string newName = Request.Query["name"];
                        string newDesc = Request.Query["desc"];
                        if (!string.IsNullOrEmpty(newName) || !string.IsNullOrEmpty(newDesc))
                        {
                            EditInfo(Path.Combine(projectPath, path), newName, newDesc);
                        }
                    }
                    else if (folders.Length == 4)
                    {
                        string to = Request.Query["to"];
                        string topicPath = path.Replace(folders[3], "topics");
                        if (!string.IsNullOrEmpty(to))
                        {
                            EditTopicIndex(Path.Combine(projectPath, topicPath), folders[3], int.Parse(to));
                        }
                        else if (Request.Form.Files.Count > 0)
                        {
                            string newPathToZip = Path.Combine(projectPath,
                                path.Replace(folders[3],
                                    Path.Combine("default", "pub_in", Request.Form.Files[0].FileName)));
                            if (!IOFile.Exists(newPathToZip))
                            {
                                using (var fileStream = new FileStream(newPathToZip, FileMode.Create))
                                {
                                    Request.Form.Files[0].CopyTo(fileStream);
                                }

                                EditZipPath(Path.Combine(projectPath, topicPath, folders[3] + ".info"), newPathToZip);
                            }
                        }
                        else
                        {
                            return Content("wrong data");
                        }
                    }
                }
            }

            return new StatusCodeResult(302);
        }
        
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
                    IOFile.WriteAllText(filename, json);
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

        public static bool RemoveFolder(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    IOFile.Delete(file);
                }
                foreach (string dir in Directory.GetDirectories(path))
                {
                    RemoveFolder(dir);
                }
                Directory.Delete(path);
                return true;
            }
            return false;
        }

        void RemoveTopic(string path, string topic)
        {
            string info = Path.Combine(path, "topics", topic + ".info");
            if (IOFile.Exists(info))
            {
                TopicModel model = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(info));
                string[] localizations = new string[] { "default", "en_en", "ru_ru" };
                foreach (string loc in localizations)
                {
                    model.localization = loc;
                    if (!string.IsNullOrEmpty(model.pathToZip))
                    {
                        IOFile.Delete(model.pathToZip);
                    }
                    if (!string.IsNullOrEmpty(model.vmpPath))
                    {
                        IOFile.Delete(model.vmpPath);
                        IOFile.Delete(model.vmpPath.Replace(".vmp", ".vmb"));
                    }
                }
                IOFile.Delete(info);
            }
        }

        void EditInfo(string path, string newName)
        {
            string pathToInfo = Path.Combine(path, new DirectoryInfo(path).Name + ".info");
            if (IOFile.Exists(pathToInfo))
            {
                CategoryModel model = JsonConvert.DeserializeObject<CategoryModel>(IOFile.ReadAllText(pathToInfo));
                model.title = newName;
                IOFile.WriteAllText(pathToInfo, JsonConvert.SerializeObject(model));
            }
        }

        void EditInfo(string path, string newName, string newDesc)
        {
            string pathToInfo = Path.Combine(path, new DirectoryInfo(path).Name + ".info");
            if (IOFile.Exists(pathToInfo))
            {
                DescModel model = JsonConvert.DeserializeObject<DescModel>(IOFile.ReadAllText(pathToInfo));
                if (!string.IsNullOrEmpty(newName))
                    model.title = newName;
                if (!string.IsNullOrEmpty(newDesc))
                    model.desc = newDesc;
                IOFile.WriteAllText(pathToInfo, JsonConvert.SerializeObject(model));
            }
        }

        void EditZipPath(string topic, string path)
        {
            if (IOFile.Exists(topic))
            {
                TopicModel model = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(topic));
                model.localization = "default";
                model.pathToZip = path;
                IOFile.WriteAllText(topic, JsonConvert.SerializeObject(model));
            }
        }

        void EditTopicIndex(string path, string topic, int to)
        {
            topic = Path.Combine(path, topic + ".info").Replace('\\', '/');

            if (IOFile.Exists(topic))
            {
                TopicModel baseModel = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(topic));
                int from = baseModel.index;
                if (from == to)
                    return;
                baseModel.index = to;
                foreach (string file in Directory.GetFiles(path))
                {

                    if (file.Replace('\\', '/') != topic)
                    {
                        TopicModel model = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(file));
                        if (model.index > from && model.index <= to && from < to)
                        {
                            model.index--;
                            IOFile.WriteAllText(file, JsonConvert.SerializeObject(model));
                        }
                        else if (model.index < from && model.index >= to && from > to)
                        {
                            model.index++;
                            IOFile.WriteAllText(file, JsonConvert.SerializeObject(model));
                        }
                    }
                }
                IOFile.WriteAllText(topic, JsonConvert.SerializeObject(baseModel));
            }
        }
    }
}