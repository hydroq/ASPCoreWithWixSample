using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Extensions.Logging;

namespace REFLEKT.ONEAuthor.Application.Helpers
{
    public class Publishing
    {
        private static string currentPubDir = "";
        private static string pathToCurrentFile = "";
        private static Action<string> onResultAction;

        private static readonly ILogger _logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(Publishing));

        public static void Draft(string path, Action<string> onResult)
        {
            onResultAction = onResult;
            foreach (string s in Directory.GetFiles(Path.Combine(path, "default", "pub_in")))
            {
                if (!Path.GetExtension(s).Contains("zip") && !Path.GetExtension(s).Contains("png") && !Path.GetExtension(s).Contains("jpg"))
                {
                    File.Delete(s);
                }
            }

            foreach (string s in Directory.GetFiles(Path.Combine(path, "default", "pub_draft")))
            {
                if (!Path.GetExtension(s).Contains("html"))
                {
                    File.Delete(s);
                }
            }

            string pathToInput = PathHelper.GetUserProcessingFolder();

            if (!Directory.Exists(Path.Combine(pathToInput, "input")))
            {
                Directory.CreateDirectory(Path.Combine(pathToInput, "input"));
            }
            foreach (string s in Directory.GetFiles(Path.Combine(pathToInput, "input")))
            {
                File.Delete(s);
            }
            
            if (!Directory.Exists(Path.Combine(pathToInput, "input/resourceFolder")))
            {
                Directory.CreateDirectory(Path.Combine(pathToInput, "input/resourceFolder"));
            }
            foreach (string s in Directory.GetFiles(Path.Combine(pathToInput, "input/resourceFolder")))
            {
                File.Delete(s);
            }

            Thread.Sleep(500);

            try
            {
                List<TopicModel> topics = new List<TopicModel>();
                foreach (string s in Directory.GetFiles(Path.Combine(path, "topics")))
                {
                    TopicModel m = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(s));
                    m.infoPath = s;
                    topics.Add(m);
                }

                topics = topics.OrderBy(t => t.index).ToList();

                foreach (TopicModel s in topics)
                {
                    DraftTopic(s.infoPath, path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while exporting topics to draft");
            }

            Thread.Sleep(500);

            GenerateXml(path);
        }

        public static void Publish(string path, Action<string> onResult)
        {
            onResultAction = onResult;
            string[] drafts = Directory.GetFiles(Path.Combine(path, "default", "pub_draft"));

            foreach (string s in Directory.GetFiles(Path.Combine(path, "default", "pub_out")))
            {
                if (!Path.GetExtension(s).Contains("html"))
                {
                    File.Delete(s);
                }
            }

            foreach (string s in drafts)
            {
                File.Copy(s, s.Replace("pub_draft", "pub_out"), true);
                File.Delete(s);
            }

            SVNManager.Commit("Save project", (res) =>
            {
                onResultAction("");
            });
        }

        private static void DraftTopic(string topic, string path)
        {
            string pathToFolder = Path.Combine(path, "default", "projects");
            TopicModel topicModel = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(topic));
            topicModel.localization = "default";
            string vmbFile = topicModel.vmpPath.Replace(".vmp", ".vmb");
            if (File.Exists(vmbFile))
            {
                string unzipFolder = Path.Combine(pathToFolder, topicModel.name.Replace(" ", "_"));
                if (Directory.Exists(unzipFolder))
                {
                    foreach (string f in Directory.GetFiles(unzipFolder))
                    {
                        File.Delete(f);
                    }
                }

                if (!string.IsNullOrEmpty(topicModel.pathToZip) && File.Exists(topicModel.pathToZip))
                {
                    topicModel.pathToZip = topicModel.pathToZip.Replace("data\\", UsersHelper.GetToolsFolder() + "\\");
                    topicModel.pathToZip = topicModel.pathToZip.Replace("data/", UsersHelper.GetToolsFolder() + "/");
                    string toolsDir = PathHelper.GetUserProcessingFolder();
                    string pathOne = Path.Combine(toolsDir, "input/resourceFolder").Replace("\\", "/");
                    string pathTwo = Path.Combine(toolsDir, "input").Replace("\\", "/");
                    File.Copy(topicModel.pathToZip, Path.Combine(pathOne, Path.GetFileName(topicModel.pathToZip)).Replace("\\", "/"), true);
                    File.Copy(topicModel.pathToZip, Path.Combine(pathTwo, Path.GetFileName(topicModel.pathToZip)).Replace("\\", "/"), true);
                }

                Unzip(vmbFile, unzipFolder);
                ConvertToX3D(unzipFolder, Path.Combine(path, "default", "pub_draft"), topicModel.name + new Random().Next(1000, 9999), topicModel.type == "info", topicModel);
            }

            if (!string.IsNullOrEmpty(topicModel.pathToZip) && File.Exists(topicModel.pathToZip))
            {
                string fileCopy = topicModel.pathToZip.Replace("pub_in", "pub_draft");
                File.Copy(topicModel.pathToZip, fileCopy, true);
                SVNManager.AddFile(fileCopy.Replace(Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder()) + "/", ""));
            }
        }

        private static void GenerateXml(string path)
        {
            string repoPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());
            string pathToDir = path;
            string relativePath = path.Replace(repoPath, "").Replace('\\', '/');
            string[] relativeParts = relativePath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            pathToCurrentFile = Path.Combine(path, relativeParts[relativeParts.Length - 1] + ".info");
            string title = JsonConvert.DeserializeObject<PublishedScenario>(File.ReadAllText(Path.Combine(path, relativeParts[relativeParts.Length - 1] + ".info"))).title;
            path = Path.Combine(path, "default", "pub_draft");
            string fileName = relativeParts[relativeParts.Length - 3] + "_" + relativeParts[relativeParts.Length - 2] + "_" + relativeParts[relativeParts.Length - 1] + ".xml";
            path = Path.Combine(path, fileName);
            GenerateXMLFromDirectory(Path.Combine(pathToDir, "default", "pub_in"), path, title);
            XSLTTransformation(path, path);
        }

        private static void Unzip(string filePath, string location)
        {
            ZipFile.ExtractToDirectory(filePath, location, true);
        }

        private static void ConvertToX3D(string fromFolder, string copyTo, string name, bool isInfo, TopicModel model)
        {
            currentPubDir = copyTo;
            string path = PathHelper.GetUserProcessingFolder();

            foreach (string s in Directory.GetFiles(fromFolder))
            {
                if (s.ToLower().Contains(".xml"))
                {
                    string copyToFile = Path.Combine(s.Replace(Path.GetFileName(s), ""), name.Replace(" ", "_") + ".xml");
                    File.Copy(s, copyToFile, true);
                }
            }

            foreach (string s in Directory.GetFiles(fromFolder))
            {
                if (s.ToLower().Contains("x3d") && !isInfo)
                {
                    File.Copy(s, Path.Combine(path, "input/" + name.Replace(" ", "_") + ".x3d"), true);
                }
                else if (s.ToLower().Contains("interactivity.xml") && !isInfo)
                {
                    string texts = File.ReadAllText(s);
                    texts = texts.Replace("temp", name.Replace(" ", "_"));
                    File.WriteAllText(s, texts);
                    File.Copy(s, Path.Combine(path, "input/" + name.Replace(" ", "_") + ".ixml"), true);
                }
                else if (!s.ToLower().Contains("interactivity.xml") && s.ToLower().Contains(".xml"))
                {
                    string text = File.ReadAllText(s);
                    if (text.Contains("<SimulationInformation>"))
                        continue;
                    if (model.type == "poi")
                    {
                        string poiAttach = "<media-3d-poi href=\"res://" + name.Replace(" ", "_") + ".x3d\" />\n";
                        string zipAttach = "";

                        if (!string.IsNullOrEmpty(model.pathToZip))
                        {
                            zipAttach = "<tracking-config-poi href='res://" + Path.GetFileName(model.pathToZip).Replace(" ", "_") + "' />\n";
                        }

                        text = GenerateIdFor(text, "<title>", 13, "<title id=\"CAP0e6ba25c-a8a1-45fb-a957-{0}\">");
                        text = GenerateIdFor(text, "<poi>", 13, "<poi id=\"CAP9998d1d5-771b-42d9-b979-{0}\">");
                        text = text.Replace("<poi-links></poi-links>", "<poi-links>" + poiAttach + "" + zipAttach + "</poi-links>");
                        if (text.Contains("POILINKS"))
                        {
                            text = text.Replace("POILINKS", poiAttach + "" + zipAttach);
                        }
                        else
                        {
                            if (text.Contains("media-3d-poi"))
                            {
                                text = text.Replace("</poi-links>", zipAttach + "</poi-links>");
                            }
                            else
                            {
                                text = text.Replace("</poi-links>", poiAttach + zipAttach + "</poi-links>");
                            }
                        }
                        if (!string.IsNullOrEmpty(zipAttach))
                        {
                            text = text.Replace(zipAttach + zipAttach, zipAttach);
                        }
                    }
                    else if (model.type == "animation")
                    {
                        string animAttach = "<media-3d href=\"res://" + name.Replace(" ", "_") + ".x3d\" />\n";
                        string zipAttach = "";
                        if (!string.IsNullOrEmpty(model.pathToZip) && File.Exists(model.pathToZip))
                        {
                            zipAttach = "<tracking-config href='res://" + Path.GetFileName(model.pathToZip).Replace(" ", "_") + "' />\n";
                        }
                        text = GenerateIdFor(text, "<title>", 13, "<title id=\"CAP0e6ba25c-a8a1-45fb-a957-{0}\">");
                        text = GenerateIdFor(text, "<step>", 12, "<step id=\"CAP0e6ba25c-a8a1-45fb-a957-{0}\">");
                        text = GenerateIdFor(text, "<info>", 12, "<info id=\"CAP0e6ba25c-a8a1-45fb-a957-{0}\">");

                        int infoId = 1;
                        bool infoContains = text.Contains("<info>");
                        while (infoContains)
                        {
                            text = GenerateIdFor(text, "<info>", 12, "<info id=\"CAP0e6ba25c-a8a1-45fb-a957-{0}\">");
                            infoId += 1;
                            infoContains = text.Contains("<info>");
                        }

                        if (text.Contains("ANIMLINKS"))
                        {
                            text = text.Replace("ANIMLINKS", animAttach + "" + zipAttach);
                        }
                        else
                        {
                            if (text.Contains("media-3d"))
                            {
                                text = text.Replace("</animation-links>", zipAttach + "</animation-links>");
                            }
                            else
                            {
                                text = text.Replace("</animation-links>", animAttach + zipAttach + "</animation-links>");
                            }
                        }
                        if (!string.IsNullOrEmpty(zipAttach))
                        {
                            text = text.Replace(zipAttach + zipAttach, zipAttach);
                        }
                    }
                    else
                    {
                        text = GenerateIdFor(text, "<title>", 13, "<title id=\"CAP0e6ba25c-a8a1-45fb-a957-{0}\">");
                    }
                    text = text.Replace("res://res://", "res://");

                    string[] tags = new string[] { "image", "video", "manual-link" };
                    foreach (string tag in tags)
                    {
                        text = ParseLinks(text, currentPubDir, tag, fromFolder);
                        text = text.Replace("< " + tag, "<" + tag);
                    }

                    File.WriteAllText(s, text);
                    File.Copy(s, Path.Combine(copyTo.Replace("pub_draft", "pub_in"), name.Replace(" ", "_") + ".xml"), true);
                }
                else if (
                    Path.GetExtension(s).Contains("jpg") ||
                    Path.GetExtension(s).Contains("jpeg") ||
                    Path.GetExtension(s).Contains("png") ||
                    Path.GetExtension(s).Contains("pdf") ||
                    Path.GetExtension(s).Contains("svg") ||
                    Path.GetExtension(s).Contains("mp4") ||
                    Path.GetExtension(s).Contains("ogg")
                    )
                {
                    if (!File.Exists(Path.Combine(copyTo, Path.GetFileName(s))))
                        File.Copy(s, Path.Combine(copyTo, Path.GetFileName(s)));
                }
            }
        }

        private static string ParseLinks(string text, string pathForImg, string tag, string archiveFolder)
        {
            string matchString = string.Format("<{0} [^>]+>+[^/]+/" + tag + ">", tag);
            Match match = Regex.Match(text, matchString);
            while (match.Success)
            {
                string val = match.Value;
                string imgPath = "";
                int indexOf = val.IndexOf("href=\"") + 6;
                for (int i = indexOf; i < val.Length; i++)
                {
                    if (val[i] != '\"')
                    {
                        imgPath += val[i];
                    }
                    else
                    {
                        break;
                    }
                }

                if (File.Exists(imgPath.Replace("%20", " ")))
                {
                    string newImageName = Path.GetFileName(imgPath.Replace("%20", " "));
                    if (!File.Exists(Path.Combine(pathForImg, newImageName)))
                        File.Copy(imgPath, Path.Combine(pathForImg, newImageName));
                    string newVal = val.Replace(imgPath, newImageName).Replace("<" + tag, "< " + tag);
                    text = text.Replace(val, newVal);
                }
                else if (imgPath.Contains("../"))
                {
                    string pathToNewFile = Path.Combine("C:/ProgramData/ParallelGraphics/VM/TC_Cache", imgPath);
                    string fullPath = Path.GetFullPath(pathToNewFile.Replace("%20", " "));
                    if (File.Exists(fullPath))
                    {
                        fullPath = fullPath.Replace("\\", "/");
                        string newImageName = Path.GetFileName(fullPath);
                        if (!File.Exists(Path.Combine(pathForImg, newImageName)))
                            File.Copy(fullPath, Path.Combine(pathForImg, newImageName));
                        string newVal = val.Replace(imgPath, newImageName).Replace("<" + tag, "< " + tag);
                        text = text.Replace(val, newVal);
                    }
                    else
                        text = text.Replace(val, val.Replace("<" + tag, "< " + tag)).Replace(imgPath, imgPath);
                }
                else
                {
                    text = text.Replace("data\\", UsersHelper.GetToolsFolder() + "\\");
                    text = text.Replace("data/", UsersHelper.GetToolsFolder() + "/");
                    text = text.Replace(val, val.Replace("<" + tag, "< " + tag)).Replace(imgPath, imgPath);
                }
                match = Regex.Match(text, matchString);
            }

            return text;
        }

        private static string GenerateIdFor(string inputText, string generateFor, int count, string template)
        {
            string text = inputText;
            while (text.IndexOf(generateFor) != -1)
            {
                int index = text.IndexOf(generateFor);
                string firstPart = text.Substring(0, index);
                string second = text.Substring(index + generateFor.Length, text.Length - (index + generateFor.Length));
                string newPart = string.Format(template, GenerateId(count));
                text = firstPart + newPart + second;
            }

            return text;
        }

        private static Random random = new Random();

        private static string GenerateId(int count)
        {
            char[] chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            string res = "";
            for (int i = 0; i < count; i++)
            {
                res += chars[random.Next(0, chars.Length)];
            }
            return res;
        }

        private static void GenerateXMLFromDirectory(string path, string writeTo, string title)
        {
            //path = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder(), path);

            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
            string xmlMask = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<topic>\r\n<title>{0}</title>\r\n{1}\r\n</topic>";
            string xmlParts = "";
            foreach (FileInfo file in files)
            {
                string fileName = Path.GetFileName(file.FullName);
                if (fileName != "dita_pub_00001.xml" && Path.GetExtension(file.FullName).Contains("xml"))
                {
                    string[] lines = File.ReadAllLines(file.FullName);
                    for (int i = 2; i < lines.Length; i++)
                    {
                        xmlParts += lines[i] + "\r\n";
                    }
                }
            }

            string result = string.Format(xmlMask, title, xmlParts);
            File.WriteAllText(writeTo, result, System.Text.Encoding.UTF8);

            SVNManager.AddFile(writeTo, (res) => { SVNManager.Commit("Generate xml files", (com) => { }); });
        }

        public static string CopyTo = "";

        private static void XSLTTransformation(string fromFolder, string copyTo)
        {
            string path = PathHelper.GetUserProcessingFolder();
            DirectoryInfo dir = new DirectoryInfo(path);
            path = dir.FullName.Replace("\\", "/");
            string aPath = Path.Combine(path, "xslt/a.xml").Replace("\\", "/");

            if (!Directory.Exists(Path.GetDirectoryName(aPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(aPath));
            }

            File.Copy(fromFolder, Path.Combine(path, "xslt/a.xml"), true);
            File.Copy(Path.Combine(path, "xslt/a.xml"), Path.Combine(path, "input/1_1.xml"), true);
            string command = "";

            var xsltConfigFileName = Path.Combine(path, "xslt.config");
            if (!File.Exists(xsltConfigFileName))
            {
                File.WriteAllText(xsltConfigFileName, JsonConvert.SerializeObject(new XsltConfigModel { port = 9000, ticket = "ytrewq654321", url = "http://127.0.0.1" }));
            }

            string configContent = File.ReadAllText(xsltConfigFileName);

            var xspConfigFilePath = Path.Combine(path, "xsp.config");
            if (!File.Exists(xspConfigFilePath))
            {
                File.WriteAllText(xspConfigFilePath, JsonConvert.SerializeObject(new XSPConfigModel { port = 0 }));
            }
            string xspContent = File.ReadAllText(Path.Combine(path, "xsp.config"));
            XsltConfigModel config = JsonConvert.DeserializeObject<XsltConfigModel>(configContent);
            XsltConfigModel xspConfig = JsonConvert.DeserializeObject<XsltConfigModel>(xspContent);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(copyTo.Replace("\\", "/"));
            string parameters = "id=" + fileNameWithoutExtension + " server_host={SERVER}:{PORT} ticket=" + config.ticket;
            CopyTo = copyTo;
            Action onFinish = () =>
            {
                foreach (string s in Directory.GetFiles(path))
                {
                    if (s.ToLower().Contains("b.xml"))
                    {
                        string textToReplace = File.ReadAllText(s);
                        while (textToReplace.Contains("res://res://"))
                        {
                            textToReplace = textToReplace.Replace("res://res://", "res://");
                        }
                        textToReplace = textToReplace.Replace("/document", "/viewer/document");
                        textToReplace = textToReplace.Replace(config.ticket, "{TICKET}");
                        File.WriteAllText(copyTo, textToReplace);
                        File.Copy(copyTo, Path.Combine(path, "input/1_1.xml"), true);
                        string repoPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());
                        SVNManager.AddFile(copyTo.Replace(repoPath + "/", ""));
                    }
                }

                X3DTrans(copyTo);
            };

            Thread th = new Thread(new ThreadStart(() =>
            {
                command = "cd \"" + path + "\" & java -jar \"" + PathHelper.GetToolsPath() + "/saxon9he.jar\" \"" + path + "/xslt/a.xml\" \"" + PathHelper.GetToolsPath() + "/xslt/c.xsl\" " + parameters + " > \"" + path + "/b.xml\"";
                ProcessStartInfo processInfo;
                processInfo = new ProcessStartInfo("cmd.exe", "/C " + command);

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;

                var process = Process.Start(processInfo);
                string outputt = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(errors))
                {
                    _logger.LogError($"Error executing command: \"{command}\". Message: '{errors}'");
                }

                process.WaitForExit();
                onFinish();
            }));
            th.Start();
        }

        private static void X3DTrans(string copyTo)
        {
            string basePath = AppContext.BaseDirectory.Replace("\\", "/");

            string binaryToolsPath = PathHelper.GetToolsPath();
            string userProcessingPath = PathHelper.GetUserProcessingFolder();

            string command = "cd \"" + userProcessingPath + "\" & \"" + binaryToolsPath + "/PublishEngineExecute.exe\" \"" + userProcessingPath + "/input\" \"" + userProcessingPath + "/input/resourceFolder\" -p XmlCompass -f";

            ProcessStartInfo processInfo;
            Action onFinish = () =>
            {
                string pathToFile = "";
                string[] files = Directory.GetFiles(Path.Combine(userProcessingPath, "input/resourceFolder"));

                foreach (string fileName in files)
                {
                    if (fileName.ToLower().Contains("x3d") || fileName.ToLower().Contains("zip") || fileName.ToLower().Contains("rar"))
                    {
                        string copyFile = Path.Combine(copyTo.Replace(Path.GetFileName(copyTo), ""), Path.GetFileName(fileName));
                        File.Copy(fileName, copyFile, true);
                        string repoPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());
                        SVNManager.AddFile(copyFile.Replace(repoPath + "/", ""));
                        pathToFile = copyFile;
                        if (fileName.ToLower().Contains("x3d"))
                        {
                            _logger.LogInformation($"Processing X3D file: '{fileName}'");

                            DirectoryInfo dInfo = new DirectoryInfo(binaryToolsPath);
                            string newDPath = dInfo.FullName.Replace("\\", "/");
                            string x3dCommand = "cd \"" + PathHelper.GetUserProcessingFolder() + "\" & \"" + newDPath + "/remgeom.bat\" \"" + copyFile + "\" \"" + newDPath + "\"";
                            ProcessStartInfo processBat = new ProcessStartInfo("cmd.exe", "/C " + x3dCommand);

                            processBat.CreateNoWindow = true;
                            processBat.UseShellExecute = false;
                            processBat.RedirectStandardOutput = true;
                            processBat.RedirectStandardError = true;
                            processBat.WorkingDirectory = newDPath;

                            var processB = Process.Start(processBat);
                            string outputt = processB.StandardOutput.ReadToEnd();
                            string errors = processB.StandardError.ReadToEnd();

                            if (!string.IsNullOrEmpty(errors))
                            {
                                _logger.LogError($"Error while performing X3D transformation: '{errors}' Command: {x3dCommand}");
                            }

                            processB.WaitForExit();
                        }
                    }
                }

                foreach (string s in Directory.GetFiles(Path.Combine(userProcessingPath, "input")))
                {
                    if (s.ToLower().Contains(".xml"))
                    {
                        string ditaText = File.ReadAllText(s);

                        File.WriteAllText(s, ditaText);
                        File.Copy(s, CopyTo, true);
                    }
                }

                OnFinishDrafting();

                onResultAction("");
            };

            processInfo = new ProcessStartInfo("cmd.exe", "/C " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            var process = Process.Start(processInfo);
            string output = process.StandardOutput.ReadToEnd();
            string errorss = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errorss))
            {
                _logger.LogError($"Error '{errorss}' while executing command: {command} ");
            }

            process.WaitForExit();

            command = "cd \"" + userProcessingPath + "\" & \"" + binaryToolsPath + "/PublishEngineExecute.exe\" \"" + userProcessingPath + "/input\" \"" + userProcessingPath + "/input/resourceFolder\" -p Cortona –f";

            processInfo = new ProcessStartInfo("cmd.exe", "/C " + command);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            process = Process.Start(processInfo);
            output = process.StandardOutput.ReadToEnd();
            errorss = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errorss))
            {
                _logger.LogError($"Error '{errorss}' while executing command: {command} ");
            }

            process.WaitForExit();
            onFinish();
        }

        private static void OnFinishDrafting()
        {
            string pathToFile = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder(), pathToCurrentFile);
            string pathToDefault = Path.Combine(pathToFile.Replace(Path.GetFileName(pathToFile), ""), "default");
            string projectsDir = Path.Combine(pathToDefault, "projects");

            foreach (var file in Directory.GetFiles(projectsDir))
            {
                if (file.Contains(".vmb") && file.Contains(".vmp"))
                {
                    File.Delete(file);
                }
            }

            foreach (var dir in Directory.GetDirectories(projectsDir))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}