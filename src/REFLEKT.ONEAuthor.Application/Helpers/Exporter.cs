using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using REFLEKT.ONEAuthor.Application.Utilities;

namespace REFLEKT.ONEAuthor.Application.Helpers
{
    public class Exporter
    {
        private static List<IdContainer> tempIds = new List<IdContainer>();
        private static readonly ILogger _logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(Exporter));

        public static void ReplaceFileInZip(string zipPath, string replacedFile, bool isVmp = true)
        {
            TempFolderCreate();
            string tempPath = GetTempFolderPath();

            ZipFile.ExtractToDirectory(zipPath, tempPath, true);
            
            if (isVmp)
            {
                foreach (string s in Directory.GetFiles(tempPath))
                {
                    if (s.Contains(".xml") && !s.Contains("Manuals.xml"))
                    {
                        string content = File.ReadAllText(s);
                        if (!content.Contains("<manual"))
                        {
                            File.Copy(replacedFile, s, true);
                        }
                    }
                }
            }
            else
            {
                File.Copy(replacedFile, Path.Combine(tempPath, "temp.xml"), true);
            }

            ZipFile.CreateFromDirectory(tempPath, zipPath);

            PathHelper.DeleteDirectory(GetTempFolderPath());
        }

        public static void Import(string res, string location, bool overwrite, Action<ImportedScenarioModel> onResult)
        {
            if (string.IsNullOrEmpty(res))
            {
                return;
            }

            var fileInfo = new FileInfo(res);
            if (fileInfo.Extension != ".zip")
            {
                return;
            }

            string repoPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());

            if (location.Contains(repoPath))
            {
                location = location.Replace(repoPath, "");
            }

            location = location.Replace("\\", "/");
            TempFolderCreate();
            ZipFile.ExtractToDirectory(res, GetTempFolderPath(), true);

            tempIds.Clear();
            Thread.Sleep(1000);
            ImportHandler(0, "", GetTempFolderPath(), overwrite, location);

            foreach (DirectoryInfo dir in new DirectoryInfo(GetTempFolderPath()).GetDirectories())
            {
                string newLocation = dir.FullName.Replace("\\", "/")
                    .Replace(GetTempFolderPath().Replace("\\", "/"), repoPath);
                MoveFolder(dir.FullName.Replace("\\", "/"), newLocation.Replace("\\", "/"));
            }

            ImportedScenarioModel model = new ImportedScenarioModel();
            model.scenarios_ids = new List<string[]>();

            //string json = "{ scenarios_ids: [ ";
            foreach (var i in tempIds)
            {
                string[] scenario = new string[2];
                scenario[0] = i.oldId;
                scenario[1] = i.newId;
                //json += string.Format("[\"{0}\",\"{1}\"],", i.oldId, i.newId);
                model.scenarios_ids.Add(scenario);
            }

            //json = json.Remove(json.Length - 1);
            //json += "]}";
            PathHelper.DeleteDirectory(GetTempFolderPath());
            onResult(model);
        }

        public static void ImportTopic(string res, string location, bool overwrite, Action<ImportedTopicModel> onResult)
        {
            if (string.IsNullOrEmpty(res))
                return;

            string repoPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder()).Replace("\\", "/");
            location = location.Replace("\\", "/");
            if (location.Contains(repoPath))
            {
                location = location.Replace(repoPath, "");
            }

            location = location.Replace("\\", "/");

            TempFolderCreate();
            var topicTempFoler = Path.Combine(GetTempFolderPath(), new FileInfo(res).Name + DateTime.Now.Millisecond);
            
            ZipFile.ExtractToDirectory(res, topicTempFoler, true);
            
            tempIds.Clear();

            ImportHandler(0, "", topicTempFoler, overwrite, location, true);
            List<string> directories = GetDirectories(new DirectoryInfo(topicTempFoler).FullName, "*", SearchOption.AllDirectories);
            foreach (string dir in directories)
            {
                if (dir.Contains("topics") || dir.Contains("default"))
                {
                    string newLocation = dir.Replace("\\", "/").Replace(topicTempFoler.Replace("\\", "/"), repoPath);
                    MoveFolder(dir.Replace("\\", "/"), newLocation.Replace("\\", "/"));
                }
            }

            PathHelper.DeleteDirectory(topicTempFoler);

            ImportedTopicModel model = new ImportedTopicModel();

            onResult(model);
        }

        public static List<string> GetDirectories(string path, string searchPattern = "*",
         SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectories(directories[i], searchPattern));

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        public static void Export(string res, string file, ExportType exportType)
        {
            if (string.IsNullOrEmpty(res))
                return;

            string root = file.Replace(PathHelper.GetRepoPath(), "").Replace("\\", "/");
            string[] rootParts = root.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (!file.Contains(PathHelper.GetRepoPath()))
            {
                file = Path.Combine(PathHelper.GetRepoPath(), file);
            }

            TempFolderCreate();

            List<PathContainer> files = new List<PathContainer>();

            if (exportType == ExportType.Topic)
            {
                files = GetFilesTopic(file, Path.Combine(rootParts[0], rootParts[1], rootParts[2])).ToList();
                string categoryInfo = Path.Combine(rootParts[0], rootParts[0] + ".info");
                files.Add(new PathContainer(Path.Combine(PathHelper.GetRepoPath(), categoryInfo), categoryInfo));
                string productInfo = Path.Combine(rootParts[0], rootParts[1], rootParts[1] + ".info");
                files.Add(new PathContainer(Path.Combine(PathHelper.GetRepoPath(), productInfo), productInfo));
                string scenarioInfo = Path.Combine(rootParts[0], rootParts[1], rootParts[2], rootParts[2] + ".info");
                files.Add(new PathContainer(Path.Combine(PathHelper.GetRepoPath(), scenarioInfo), scenarioInfo));
            }
            else if (exportType == ExportType.Scenario)
            {
                files = GetFilesScenario(file, Path.Combine(rootParts[0], rootParts[1])).ToList();
                string categoryInfo = Path.Combine(rootParts[0], rootParts[0] + ".info");
                files.Add(new PathContainer(Path.Combine(PathHelper.GetRepoPath(), categoryInfo), categoryInfo));
                string productInfo = Path.Combine(rootParts[0], rootParts[1], rootParts[1] + ".info");
                files.Add(new PathContainer(Path.Combine(PathHelper.GetRepoPath(), productInfo), productInfo));
            }
            else if (exportType == ExportType.Product)
            {
                files = GetFilesProduct(file, rootParts[0]).ToList();
                string categoryInfo = Path.Combine(rootParts[0], rootParts[0] + ".info");
                files.Add(new PathContainer(Path.Combine(PathHelper.GetRepoPath(), categoryInfo), categoryInfo));
            }
            else if (exportType == ExportType.Category)
            {
                files = GetFilesCategory(file).ToList();
            }
            else if (exportType == ExportType.All)
            {
                files = GetFilesRepository().ToList();
            }

            foreach (PathContainer f in files)
            {
                string tempName = new FileInfo(f.relative).Name;
                string dirName = f.relative.Replace(tempName, "");
                Directory.CreateDirectory(Path.Combine(GetTempFolderPath(), dirName));
                if (File.Exists(f.source))
                {
                    File.Copy(f.source, Path.Combine(GetTempFolderPath(), f.relative), true);
                }
            }

            string zipPath = res + ".zip";

            if (!Directory.Exists(Path.GetDirectoryName(zipPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(zipPath));
            }

            try
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting existing folder export ZIP.", ex);
            }

            ZipFile.CreateFromDirectory(GetTempFolderPath(), zipPath); 
            
            PathHelper.DeleteDirectory(GetTempFolderPath());
        }

        private static void TempFolderCreate()
        {
            if (!Directory.Exists(GetTempFolderPath()))
            {
                Directory.CreateDirectory(GetTempFolderPath());
            }
        }

        private static IEnumerable<PathContainer> GetFilesRepository()
        {
            string repoDir = PathHelper.GetRepoPath();

            string[] categoryDirs = Directory.GetDirectories(repoDir);

            foreach (string c in categoryDirs)
            {
                if (PathHelper.IsProject(c))
                {
                    List<PathContainer> categorys = GetFilesCategory(PathHelper.GetFilePath(c)).ToList();

                    foreach (PathContainer p in categorys)
                    {
                        yield return p;
                    }
                }
            }
        }

        private static IEnumerable<PathContainer> GetFilesCategory(string filePath)
        {
            string category = new FileInfo(filePath).Directory.Name;
            string categoryDir = new FileInfo(filePath).Directory.FullName;

            string[] productDirs = Directory.GetDirectories(categoryDir);

            foreach (string d in productDirs)
            {
                if (PathHelper.IsProject(d))
                {
                    List<PathContainer> products = GetFilesProduct(PathHelper.GetFilePath(d)).ToList();

                    foreach (PathContainer p in products)
                    {
                        yield return new PathContainer(p.source, Path.Combine(category, p.relative));
                    }
                }
            }

            yield return new PathContainer(filePath, Path.Combine(category, category + ".info"));
        }

        private static IEnumerable<PathContainer> GetFilesProduct(string filePath, string root = "")
        {
            string product = new FileInfo(filePath).Directory.Name;
            string productDir = new FileInfo(filePath).Directory.FullName;

            string[] scenarioDirs = Directory.GetDirectories(productDir);

            foreach (string d in scenarioDirs)
            {
                if (PathHelper.IsProject(d))
                {
                    List<PathContainer> scenarios = GetFilesScenario(PathHelper.GetFilePath(d)).ToList();

                    foreach (PathContainer s in scenarios)
                    {
                        yield return new PathContainer(s.source, Path.Combine(root, product, s.relative));
                    }
                }
            }

            yield return new PathContainer(filePath, Path.Combine(root, product, product + ".info"));
        }

        private static IEnumerable<PathContainer> GetFilesScenario(string filePath, string root = "")
        {
            string scenario = new FileInfo(filePath).Directory.Name;
            string scenarioDir = new FileInfo(filePath).Directory.FullName;
            string[] topicNames = Directory.GetFiles(Path.Combine(scenarioDir, "topics"), "*.info");

            foreach (string n in topicNames)
            {
                List<PathContainer> topics = GetFilesTopic(n).ToList();

                foreach (PathContainer t in topics)
                {
                    yield return new PathContainer(t.source, Path.Combine(root, scenario, t.relative));
                }
            }

            //create pub_offline or clean
            string offlineDir = Path.Combine(scenarioDir, "default", "pub_offline");
            if (Directory.Exists(offlineDir))
            {
                PathHelper.DeleteDirectory(offlineDir);
            }
            Directory.CreateDirectory(offlineDir);
            Directory.CreateDirectory(Path.Combine(offlineDir, "Resource"));

            foreach (string f in Directory.GetFiles(Path.Combine(scenarioDir, "default", "pub_out")))
            {
                if (new FileInfo(f).Extension.Contains(".xml"))
                {
                    string newPath = f.Replace("pub_out", "pub_offline");
                    string text = File.ReadAllText(f);

                    text = ProcessDitaFileForOfflinePublication(text);
                    File.WriteAllText(newPath, text);

                    yield return new PathContainer(newPath, Path.Combine(root, scenario, "default", "pub_offline", new FileInfo(newPath).Name));
                }
                else
                {
                    string newPath = f.Replace("pub_out", Path.Combine("pub_offline", "Resource"));
                    File.Copy(f, newPath);

                    yield return new PathContainer(newPath, Path.Combine(root, scenario, "default", "pub_offline", "Resource", new FileInfo(newPath).Name));
                }
            }

            yield return new PathContainer(filePath, Path.Combine(root, scenario, scenario + ".info"));
        }

        private static IEnumerable<PathContainer> GetFilesTopic(string filePath, string root = "")
        {
            TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(filePath));
            string[] localizations = PathHelper.GetAvailableLocalizations();
            string scenarioFolder = filePath.Replace(new FileInfo(filePath).Name, "").Replace("topics", "");

            //main topic info
            yield return new PathContainer(filePath, Path.Combine(root, "topics", new FileInfo(filePath).Name));

            foreach (string loc in localizations)
            {
                if (Directory.Exists(Path.Combine(scenarioFolder, loc)))
                {
                    //projects
                    yield return new PathContainer("", Path.Combine(root, loc, "projects", "file.null"));

                    //pub_in
                    yield return new PathContainer("", Path.Combine(root, loc, "pub_in", "file.null"));

                    //pub_draft
                    yield return new PathContainer("", Path.Combine(root, loc, "pub_draft", "file.null"));

                    //pub_out
                    yield return new PathContainer("", Path.Combine(root, loc, "pub_out", "file.null"));

                    topic.localization = loc;
                    string pathToZip = topic.pathToZip;
                    string vmpPath = topic.vmpPath;

                    if (!string.IsNullOrEmpty(topic.vmpPathDEFAULT))
                    {
                        vmpPath = topic.vmpPathDEFAULT;
                    }

                    //zip
                    if (!string.IsNullOrEmpty(pathToZip))
                        yield return new PathContainer(pathToZip, Path.Combine(root, loc, "pub_in", new FileInfo(pathToZip).Name));

                    //vmp
                    if (!string.IsNullOrEmpty(vmpPath))
                        yield return new PathContainer(vmpPath, Path.Combine(root, loc, "projects", new FileInfo(vmpPath).Name));

                    //vmb
                    if (!string.IsNullOrEmpty(vmpPath))
                        yield return new PathContainer(vmpPath.Replace(".vmp", ".vmb"), Path.Combine(root, loc, "projects", new FileInfo(vmpPath.Replace(".vmp", ".vmb")).Name));
                }
            }
        }

        private static string GetTempFolderPath()
        {
            return Path.Combine(Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder()), "temp_export");
        }


        private static string ProcessDitaFileForOfflinePublication(string text)
        {
            text = XmlParseImages(text);
            text = XmlParsePdfs(text);
            text = XmlParseArsRouting(text);
            text = XmlParseCDataLinks(text);
            text = XmlParseResLinks(text);

            return text;
        }

        private static string XmlParseResLinks(string text)
        {
            var matches = Regex.Matches(text, @"(res:\/\/)([^""]+\.[A-Za-z\d]+)", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                text = text.Replace(match.Groups[0].Value, $"{match.Groups[1].Value}Resource/{match.Groups[2].Value}");
            }

            return text;
        }

        private static string XmlParseCDataLinks(string text)
        {
            var ditaXElement = XDocument.Parse(text);
            var cdataNodes = ditaXElement.DescendantNodes()
                                .Where(e => e.NodeType == XmlNodeType.CDATA)
                                .Select(n => n as XCData)
                                .Where(n => n != null);

            foreach (var cdataNode in cdataNodes)
            {
                cdataNode.Value = cdataNode.Value.Replace("res://", "../../Resource/");
            }

            return ditaXElement.SaveToString();
        }
        
        private static string XmlParseArsRouting(string text)
        {
            var matches = Regex.Matches(text, @"href=""ars:\/\/dita\/([^?""]+).*?""", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                var targetPdfLink = $@"href=""streamingAssets://Scenarios/{match.Groups[1].Value}.xml"""; // TODO: test ending " symbol presence
                text = text.Replace(match.Groups[0].Value, targetPdfLink);
            }

            return text;
        }

        private static string XmlParsePdfs(string text)
        {
            var matches = Regex.Matches(text, @"href=""[^""]+\\(.+?\.pdf).*?""", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                var targetPdfLink = $@"href=""../../Resource/{match.Groups[1].Value}"""; // TODO: test ending " symbol presence
                text = text.Replace(match.Groups[0].Value, targetPdfLink);
            }

            return text;
        }

        private static string XmlParseImages(string text)
        {
            MatchCollection matches = Regex.Matches(text, "(?:src=).*(?:image=)(.*?)(?:&amp|\").*(?:\")", RegexOptions.Multiline);
            foreach (Match matchFile in matches)
            {
                var targetFileLinkAttr = $"src=\"../../Resource/{matchFile.Groups[1].Value}\"";
                text = text.Replace(matchFile.Groups[0].Value, targetFileLinkAttr);
            }

            return text;
        }

        private static void MoveFolder(string source, string dest)
        {
            if (!Directory.Exists(dest))
            {
                Directory.Move(source, dest);
            }
            else
            {
                if (Directory.Exists(source))
                {
                    foreach (string dir in Directory.GetDirectories(source))
                    {
                        string newDir = dir.Replace(source, dest);
                        if (!Directory.Exists(newDir))
                        {
                            Directory.Move(dir, newDir);
                        }
                        else
                        {
                            MoveFolder(dir, newDir);
                        }
                    }
                    foreach (string file in Directory.GetFiles(source))
                    {
                        string newPath = file.Replace(source, dest);
                        if (File.Exists(newPath))
                            File.Delete(newPath);
                        File.Move(file, newPath);
                    }
                }
            }
        }

        private static Random random = new Random();

        private static void ImportHandler(int deep, string parent, string path, bool overwrite, string destination, bool checkIndex = false)
        {
            foreach (DirectoryInfo folder in new DirectoryInfo(path).GetDirectories())
            {
                string dirName = folder.Name;
                string infoPath = Path.Combine(path, dirName, dirName + ".info");

                if (File.Exists(infoPath))
                {
                    string[] nameSplit = dirName.Split('.');
                    string oldId = dirName.Split('.')[0];
                    string newId;
                    if (overwrite || deep < 2)
                    {
                        newId = oldId;
                    }
                    else
                    {
                        newId = PathHelper.GenerateId(10);
                    }
                    if (nameSplit.Length < 2)
                    {
                        newId = PathHelper.GenerateId(10) + ".Folder" + random.Next(1000, 10000);
                    }

                    bool copyInfo = true;
                    string newDirName = dirName.Replace(oldId, newId);
                    if (!string.IsNullOrEmpty(destination))
                    {
                        string[] dest = destination.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        if (dest.Length > 0)
                        {
                            newDirName = dest[0];
                            newId = dest[0].Split('.')[0];
                            destination = destination.Replace(dest[0], "");
                            copyInfo = false;
                        }
                    }

                    if (deep == 2)
                        tempIds.Add(new IdContainer(oldId, newId));

                    if (dirName != newDirName)
                    {
                        Directory.Move(Path.Combine(path, dirName), Path.Combine(path, newDirName));
                        infoPath = Path.Combine(path, newDirName, dirName + ".info");
                        if (copyInfo)
                        {
                            if (File.Exists(Path.Combine(path, newDirName, newDirName + ".info")))
                                File.Delete(Path.Combine(path, newDirName, newDirName + ".info"));
                            File.Move(infoPath, Path.Combine(path, newDirName, newDirName + ".info"));
                        }
                        else
                            File.Delete(infoPath);
                    }
                    ImportHandler(deep + 1, Path.Combine(parent, newDirName), folder.FullName.Replace(dirName, newDirName), overwrite, destination, checkIndex);
                }

                if (dirName == "topics")
                {
                    FileInfo[] folderFiles = folder.GetFiles();
                    int filesCount = 0;
                    if (checkIndex)
                    {
                        string repoPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder()).Replace("\\", "/");
                        string newLocation = Path.Combine(repoPath, parent).Replace("\\", "/") + "/topics";

                        if (!Directory.Exists(newLocation))
                        {
                            Directory.CreateDirectory(newLocation);
                        }

                        filesCount = Directory.GetFiles(newLocation, "*", SearchOption.AllDirectories).Length;
                    }
                    int indexId = 0;
                    foreach (FileInfo t in folderFiles)
                    {
                        TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(t.FullName));
                        OldTopicModel oldTopic = JsonConvert.DeserializeObject<OldTopicModel>(File.ReadAllText(t.FullName));
                        if (!string.IsNullOrEmpty(oldTopic.pathToZip))
                        {
                            topic.localization = "default";
                            topic.pathToZip = oldTopic.pathToZip;
                            //Debug.Log("Old import");
                        }
                        if (!string.IsNullOrEmpty(oldTopic.vmpPath))
                        {
                            topic.localization = "default";
                            topic.vmpPath = oldTopic.vmpPath;
                            //Debug.Log("Old import");
                        }

                        if (string.IsNullOrEmpty(topic.unique_id))
                        {
                            topic.unique_id = PathHelper.GenerateId(6);
                        }

                        if (checkIndex)
                            topic.index = filesCount;

                        string oldTopicName = t.Name;
                        string newTopicName = "topic" + PathHelper.GenerateIntId(8) + ".info";

                        topic.localization = "default";
                        string locPath = Path.Combine(Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder()), parent, "default");
                        string tempPath = Path.Combine(t.Directory.Parent.FullName, "default");

                        if (!string.IsNullOrEmpty(topic.pathToZip))
                        {
                            string oldZipName = new FileInfo(topic.pathToZip).Name;
                            string newZipName = "zip" + PathHelper.GenerateIntId(8) + ".zip";
                            if (File.Exists(Path.Combine(tempPath, "pub_in", oldZipName)))
                            {
                                if (File.Exists(Path.Combine(tempPath, "pub_in", newZipName)))
                                    File.Delete(Path.Combine(tempPath, "pub_in", newZipName));
                                File.Move(Path.Combine(tempPath, "pub_in", oldZipName), Path.Combine(tempPath, "pub_in", newZipName));
                            }
                            topic.pathToZip = Path.Combine(locPath, "pub_in", newZipName);
                            topic.pathToZip = topic.pathToZip.Replace("\\", "/");
                            topic.pathToZipDEFAULT = Path.Combine(locPath, "pub_in", newZipName);
                            topic.pathToZipDEFAULT = topic.pathToZip.Replace("\\", "/");
                        }
                        if (!string.IsNullOrEmpty(topic.vmpPath))
                        {
                            string oldVmpName = new FileInfo(topic.vmpPath).Name;
                            string newVmpName = "obj" + PathHelper.GenerateIntId(8) + ".vmp";
                            if (File.Exists(Path.Combine(tempPath, "projects", oldVmpName)))
                            {
                                if (File.Exists(Path.Combine(tempPath, "projects", newVmpName)))
                                    File.Delete(Path.Combine(tempPath, "projects", newVmpName));
                                File.Move(Path.Combine(tempPath, "projects", oldVmpName), Path.Combine(tempPath, "projects", newVmpName));
                            }
                            if (File.Exists(Path.Combine(tempPath, "projects", oldVmpName.Replace(".vmp", ".vmb"))))
                            {
                                if (File.Exists(Path.Combine(tempPath, "projects", newVmpName.Replace(".vmp", ".vmb"))))
                                    File.Delete(Path.Combine(tempPath, "projects", newVmpName.Replace(".vmp", ".vmb")));
                                File.Move(Path.Combine(tempPath, "projects", oldVmpName.Replace(".vmp", ".vmb")), Path.Combine(tempPath, "projects", newVmpName.Replace(".vmp", ".vmb")));
                            }
                            topic.vmpPath = Path.Combine(locPath, "projects", newVmpName);
                            topic.vmpPath = topic.vmpPath.Replace("\\", "/");
                            topic.vmpPathDEFAULT = Path.Combine(locPath, "projects", newVmpName);
                            topic.vmpPathDEFAULT = topic.vmpPath.Replace("\\", "/");
                        }

                        topic.isOpened = 0;

                        //change path to zip and vmp
                        File.WriteAllText(t.FullName, JsonConvert.SerializeObject(topic));
                        //topic.index = folderFiles.Length;
                        if (File.Exists(t.FullName.Replace(oldTopicName, newTopicName)))
                            File.Delete(t.FullName.Replace(oldTopicName, newTopicName));
                        Thread.Sleep(100);
                        File.Move(t.FullName, t.FullName.Replace(oldTopicName, newTopicName));
                    }

                    if (!checkIndex)
                    {
                        List<TopicModel> topicModels = new List<TopicModel>();

                        foreach (FileInfo t in folder.GetFiles())
                        {
                            TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(t.FullName));
                            topic.infoPath = t.FullName;
                            topicModels.Add(topic);
                        }

                        topicModels = topicModels.OrderBy(t => t.index).ToList();
                        for (int i = 0; i < topicModels.Count; i++)
                        {
                            if (topicModels[i].index != i)
                            {
                                topicModels[i].index = i;
                                File.WriteAllText(topicModels[i].infoPath, JsonConvert.SerializeObject(topicModels[i]));
                            }
                        }
                    }
                }
            }
        }
    }

    internal class IdContainer
    {
        public string oldId;
        public string newId;

        public IdContainer(string o, string n)
        {
            oldId = o;
            newId = n;
        }
    }

    public enum ExportType
    {
        All,
        Category,
        Product,
        Scenario,
        Topic
    }

    public struct PathContainer
    {
        public string source;
        public string relative;

        public PathContainer(string s, string r)
        {
            source = s;
            relative = r;
        }
    }
}