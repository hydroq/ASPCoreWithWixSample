using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Models;

namespace REFLEKT.ONEAuthor.Application.Helpers
{
    public class PathHelper
    {
        public static string GenerateId(int length)
        {
            string uniqueId = GenerateUniqueString(length);
            AddUniqueIdsToContainer(uniqueId);
            return uniqueId;
        }

        public static string GenerateIntId(int count)
        {
            string res = "";
            Random random = new Random();
            for (int i = 0; i < count; i++)
                res += random.Next(0, 10);
            return res;
        }

        private static string GenerateUniqueString(int length)
        {
            string filepath = UniqueIdsContainerPath();

            FileStream fl = new FileStream(filepath, FileMode.Open);
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(fl);
            fl.Close();

            System.Xml.XmlNodeList ids = doc.GetElementsByTagName("value");
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Random random = new Random();
            do
            {
                sb.Clear();
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                for (int i = 0; i < length; i++)
                {
                    sb.Append(chars[random.Next(0, chars.Length)]);
                }
            } while (!CheckUniqueId(ids, sb.ToString()));
            return sb.ToString();
        }

        private static bool CheckUniqueId(System.Xml.XmlNodeList nodes, string id)
        {
            foreach (System.Xml.XmlNode node in nodes)
            {
                if (node.InnerText == id)
                {
                    return false;
                }
            }
            return true;
        }

        private static string UniqueIdsContainerPath()
        {
            string path = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder(), "uid.xml");
            if (!File.Exists(path))
            {
                CreateUniqueIdsContainer(path);
            }
            return path;
        }

        private static void CreateUniqueIdsContainer(string filepath)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            System.Xml.XmlElement elem = doc.CreateElement("ids");
            doc.AppendChild(elem);
            doc.Save(filepath);
        }

        private static void AddUniqueIdsToContainer(string uniqueId)
        {
            string filepath = UniqueIdsContainerPath();

            FileStream fl = new FileStream(filepath, FileMode.Open);
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(fl);
            fl.Close();

            System.Xml.XmlElement elem = doc.CreateElement("value");
            elem.InnerText = uniqueId;
            doc.SelectNodes("ids")[0].AppendChild(elem);
            doc.Save(filepath);
        }

        public static void DeleteDirectory(string path)
        {
            string repoPath = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());

            if (!Directory.Exists(path) && !Directory.Exists(Path.Combine(repoPath, path)))
                return;
            if (!Directory.Exists(path) && Directory.Exists(Path.Combine(repoPath, path)))
            {
                path = Path.Combine(repoPath, path);
            }

            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path);
                }
                catch (Exception ex)
                {
                }
            }
            else if (Directory.Exists(Path.Combine(repoPath, path)))
            {
                try
                {
                    Directory.Delete(Path.Combine(repoPath, path));
                }
                catch (Exception ex)
                {
                }
            }
        }

        public static string[] GetAvailableLocalizations()
        {
            return new string[] { "default", "ru_ru", "en_en" };
        }

        public static string GetRepoPath()
        {
            return Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder()).Replace("\\", "/");
        }

        public static string FormatStringToValid(string source)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                source = source.Replace(c.ToString(), "");
            }

            foreach (char c in Path.GetInvalidPathChars())
            {
                source = source.Replace(c.ToString(), "");
            }

            source = source.Replace(" ", "");
            source = source.Replace(".", "");

            return source;
        }

        public static bool IsProject(string path)
        {
            string directoryName = new DirectoryInfo(path).Name;
            return File.Exists(Path.Combine(path, directoryName + ".info"));
        }

        public static string GetFilePath(string file)
        {
            string directoryName = new DirectoryInfo(file).Name;
            return Path.Combine(file, directoryName + ".info");
        }

        public static string RenameFolder(string folderPath, string oldName, string newName)
        {
            newName = FormatStringToValid(newName);

            string[] nameSplit = oldName.Split('.');
            string oldId = oldName.Split('.')[0];

            if (nameSplit.Length == 2)
            {
                newName = oldId;
            }
            else
            {
                newName = GenerateId(10);
            }

            if (oldName == newName)
                return oldName;

            if (Directory.Exists(folderPath.Replace(oldName, newName)))
                newName += new Random().Next(1000, 9999);

            if (!folderPath.Contains(PathHelper.GetRepoPath()))
            {
                folderPath = Path.Combine(PathHelper.GetRepoPath(), folderPath);
            }
            string oldPath = folderPath;
            //Debug.Log (folderPath);Debug.Log (oldName);Debug.Log (newName);

            //Directory.Move(folderPath, folderPath.Replace(oldName, newName));

            folderPath = folderPath.Replace(oldName, newName);
            //File.Move(Path.Combine(folderPath, oldName + ".info"), Path.Combine(folderPath, newName + ".info"));

            ChangeVmpPath(folderPath, oldName, newName);

            return newName;
        }

        public static void ChangeVmpPath(string root, string oldName, string newName)
        {
            foreach (string folder in Directory.GetDirectories(root))
            {
                string folderName = new DirectoryInfo(folder).Name;
                string newPath = Path.Combine(root, folderName);
                if (folderName == "topics")
                {
                    foreach (string f in Directory.GetFiles(newPath))
                    {
                        TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(f));

                        if (string.IsNullOrEmpty(topic.vmpPath) && !string.IsNullOrEmpty(topic.vmpPathDEFAULT))
                        {
                            topic.vmpPath = topic.vmpPathDEFAULT;
                        }

                        if (string.IsNullOrEmpty(topic.pathToZip) && !string.IsNullOrEmpty(topic.pathToZipDEFAULT))
                        {
                            topic.pathToZip = topic.pathToZipDEFAULT;
                        }

                        if (!string.IsNullOrEmpty(topic.vmpPath))
                        {
                            topic.vmpPath = topic.vmpPath.Replace(oldName, newName);
                            topic.vmpPathDEFAULT = topic.vmpPath;
                        }
                        if (!string.IsNullOrEmpty(topic.pathToZip))
                        {
                            topic.pathToZip = topic.pathToZip.Replace(oldName, newName);
                            topic.pathToZipDEFAULT = topic.pathToZip;
                        }
                        File.WriteAllText(f, JsonConvert.SerializeObject(topic));
                    }
                }
                else
                {
                    ChangeVmpPath(newPath, oldName, newName);
                }
            }
        }

        public static void ForceDeleteFolderContentRecursively(string path)
        {
            foreach (var folder in Directory.GetDirectories(path))
            {
                ForceDeleteFolderRecursively(folder);
            }

            var directoryInfo = new DirectoryInfo(path);
            foreach (var file in directoryInfo.GetFiles())
            {
                ForceDeleteFolderRecursively(file.DirectoryName);
            }
        }

        public static void ForceDeleteFolderRecursively(string path)
        {
            foreach (var folder in Directory.GetDirectories(path))
            {
                ForceDeleteFolderRecursively(folder);
            }

            var directoryInfo = new DirectoryInfo(path);
            foreach (var file in directoryInfo.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            directoryInfo.Attributes = FileAttributes.Normal;
            directoryInfo.Delete();
        }
        
        /// <summary>
        /// Generates RFOneAuthor_Data/Tools path, containing 3d party tools
        /// </summary>
        public static string GetToolsPath()
        {
#if DEBUG
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../RFOneAuthor/RFOneAuthor_Data/Tools"));
#else
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../RFOneAuthor/RFOneAuthor_Data/Tools"));
#endif
        }

        /// <summary>
        /// Generates ~/RfOneAuthor/localhost/tools_processing path
        /// </summary>
        public static string GetUserProcessingFolder()
        {
            var result = Path.Combine(GetApplicationWorkingDirectory(), "localhost/tools_processing");
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }

            return result;
        }

        /// <summary>
        /// Generates ~/RfOneAuthor/localhost/upload path
        /// </summary>
        public static string GetUploadFolder()
        {
            var result = Path.Combine(GetApplicationWorkingDirectory(), "localhost/upload");
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }

            return result;
        }

        /// <summary>
        /// Generates ~/RfOneAuthor/localhost/viewer/images path
        /// </summary>
        public static string GetViewerImagesFolder()
        {
            var result = Path.Combine(GetApplicationWorkingDirectory(), "localhost/viewer/images");
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }

            return result;
        }

        /// <summary>
        /// Get base application working directory:
        /// ~/users/user/RfOneAuthor
        /// </summary>
        public static string GetApplicationWorkingDirectory()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string reflektPath = Path.Combine(path, "RFOneAuthor");

            if (!Directory.Exists(reflektPath))
            {
                Directory.CreateDirectory(reflektPath);
            }

            return reflektPath;
        }

        /// <summary>
        /// Get Admins folder within app's working directory
        /// ~/users/user/RfOneAuthor/admins
        /// </summary>
        public static string GetAdminsWorkingDirectory()
        {
            var adminsPath = Path.Combine(GetApplicationWorkingDirectory(), "admins");
            if (!Directory.Exists(adminsPath))
            {
                Directory.CreateDirectory(adminsPath);
            }
            
            return adminsPath;
        }

        public static bool CheckIfStringIsPathCompliant(string targetPath)
        {
            return Path.GetInvalidFileNameChars().All(c => !targetPath.Contains(c));
        }

        public static readonly HashSet<string> ImageExtensions = new HashSet<string> { ".JPG", ".JPEG", ".JPE", ".BMP", ".GIF", ".PNG", ".TIFF", ".SVG", ".PDF" };

        public static bool CheckIfFileIsOfImageType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToUpperInvariant();

            return ImageExtensions.Contains(extension);
        }
    }
}