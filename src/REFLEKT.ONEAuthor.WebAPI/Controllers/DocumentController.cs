using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using IOFile = System.IO.File;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class DocumentController : Controller
    {
        private string _ticket = "";
        private bool _isDraft;
        private string currentUrl = "";
        private string _baseUrl;

        [Authorize]
        [HttpGet]
        [Route("document")] // only for web viewer, TODO: rework
        [Route("viewer/document")]
        public IActionResult GetDocument()
        {
            _baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            currentUrl = $"{_baseUrl}/viewer/document";

            string id = Request.Query["id"];
            _isDraft = id.EndsWith("_draft");
            if (_isDraft)
            {
                id = id.Substring(0, id.Length - "_draft".Length);
            }

            _ticket = Request.Query["ticket"];
            string image = Request.Query["image"];
            bool.TryParse(Request.Query["description"], out var description);

            if (description)
            {
                return GetDescription(id);
            }

            if (!string.IsNullOrEmpty(image))
            {
                return GetAllFiles(id, image);
            }

            return GetDitaFile(id);
        }

        public IActionResult GetDescription(string id)
        {
            string[] paths = GetPathToScenarioById(id);
            if (paths.Length <= 0)
            {
                return NotFound();
            }

            PublishedScenario scenario = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(paths[0]));
            string result = SetImageLinks(scenario.desc);

            return Content(result);
        }

        private IActionResult GetAllFiles(string id, string image)
        {
            string[] paths = GetPathToScenarioById(id, false);
            if (paths.Length <= 0)
            {
                return NotFound();
            }

            string publishDirectory = _isDraft
                ? Path.Combine(paths[0], "default/pub_draft/")
                : Path.Combine(paths[0], "default/pub_out/");

            string outputFile = Path.Combine(publishDirectory, image);
            if (IOFile.Exists(outputFile))
            {
                return PhysicalFile(outputFile, "application/octet-stream", Path.GetFileName(outputFile));
            }

            return NotFound();
        }

        private IActionResult GetDitaFile(string id)
        {
            string[] paths = GetPathToScenarioById(id, false);
            if (paths.Length <= 0)
            {
                return NotFound();
            }

            string publishDirectory = _isDraft
                ? Path.Combine(paths[0], "default/pub_draft/")
                : Path.Combine(paths[0], "default/pub_out/");

            string resultFileName = paths[1] + "_" + paths[2] + "_" + paths[3] + ".xml";
            string dita = Path.Combine(publishDirectory, resultFileName);
            if (!IOFile.Exists(dita))
            {
                return NotFound();
            }

            string tmpText = IOFile.ReadAllText(dita);
            tmpText = tmpText.Replace("{TICKET}", _ticket);
            tmpText = tmpText.Replace("\"/viewer/document", currentUrl);
            while (tmpText.Contains("server_") && !tmpText.Contains("&server_"))
            {
                tmpText = tmpText.Replace("server_", "&server_");
            }

            List<string> tmpImages = GetImagesInHTMLString(tmpText);
            List<string> newImages = SetCorrectImages(tmpImages, id);
            for (int i = 0; i < tmpImages.Count; i++)
            {
                tmpText = tmpText.Replace(tmpImages[i], newImages[i]);
            }

            tmpImages = GetUrlsInHTMLString(tmpText);
            newImages = SetCorrectUrls(tmpImages, id, publishDirectory);

            for (int i = 0; i < tmpImages.Count; i++)
            {
                if (newImages.Count > i)
                {
                    tmpText = tmpText.Replace(tmpImages[i], newImages[i]);
                }
            }

            tmpText = tmpText.Replace("{SERVER}:{PORT}", _baseUrl);

            IOFile.WriteAllText(dita, tmpText);

            return PhysicalFile(dita, "application/octet-stream", Path.GetFileName(dita));
        }

        private string SetImageLinks(string inputHtml)
        {
            const string pattern = @"<img\b[^\<\>]+?\bsrc\s*=\s*[""'](?<L>.+?)[""'][^\<\>]*?\>";

            foreach (Match match in Regex.Matches(inputHtml, pattern, RegexOptions.IgnoreCase))
            {
                var imageLink = match.Groups["L"].Value;
                if (IOFile.Exists(imageLink))
                {
                    IOFile.Copy(imageLink, Path.Combine(PathHelper.GetViewerImagesFolder(), Path.GetFileName(imageLink)), true);
                }

                inputHtml = inputHtml.Replace(imageLink, "images/" + Path.GetFileName(imageLink));
            }

            return inputHtml;
        }

        private List<string> GetImagesInHTMLString(string htmlString)
        {
            List<string> images = new List<string>();
            string pattern = @"<(img)\b[^>]*>";

            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(htmlString);

            for (int i = 0, l = matches.Count; i < l; i++)
            {
                images.Add(matches[i].Value);
            }

            return images;
        }

        private List<string> SetCorrectImages(List<string> imgs, string id)
        {
            List<string> images = new List<string>();
            foreach (string s in imgs)
            {
                string res = "";
                string tmp = s;
                string imageName = "";
                int indexStart = tmp.IndexOf("image=") + 6;
                for (int i = indexStart; i < tmp.Length; i++)
                {
                    if (tmp[i] == '&')
                        break;
                    imageName += tmp[i];
                }
                string url = currentUrl + "?id=" + id + "&image=" + imageName + "&ticket=" + _ticket;
                if (_isDraft)
                {
                    url = currentUrl + "?id=" + id + "_draft&image=" + imageName + "&ticket=" + _ticket;
                }
                url = XmlEscape(url);
                res = "<img src=\"" + url + "\"/>";
                images.Add(res);
            }
            return images;
        }

        private List<string> GetUrlsInHTMLString(string htmlString)
        {
            List<string> images = new List<string>();
            string pattern = @"<(a)\b[^>]*>";

            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(htmlString);

            for (int i = 0, l = matches.Count; i < l; i++)
            {
                images.Add(matches[i].Value);
            }

            return images;
        }

        private List<string> SetCorrectUrls(List<string> imgs, string id, string pubOut)
        {
            List<string> images = new List<string>();
            foreach (string s in imgs)
            {
                //if (!IsUrlEncoded(s))
                //{
                string s0 = HttpUtility.UrlDecode(s);
                string res = "";
                string tmp = s0;
                string imageName = "";
                int indexStart = tmp.IndexOf("image=") + 6;
                for (int i = indexStart; i < tmp.Length; i++)
                {
                    if (tmp[i] == '&')
                        break;
                    imageName += tmp[i];
                }
                if (imageName.Contains("https://") && imageName.Contains("http://") && !imageName.Contains("cap://"))
                {
                    if (Path.IsPathRooted(imageName))
                    {
                        if (IOFile.Exists(imageName))
                            IOFile.Copy(imageName, Path.Combine(pubOut, Path.GetFileName(imageName)));

                        imageName = Path.GetFileName(imageName);
                    }

                    string url = currentUrl + "?id=" + id + "&image=" + imageName + "&ticket=" + _ticket;
                    //if (!IsUrlEncoded(url))
                    //    url = HttpUtility.UrlEncode(url);
                    url = XmlEscape(url);
                    res = "<a href=\"" + url + "\">";
                    images.Add(res);
                }
                else
                {
                    images.Add(s);
                }
                //}
            }
            return images;
        }

        public bool IsUrlEncoded(string text)
        {
            return (HttpUtility.UrlDecode(text) != text);
        }

        public static string XmlEscape(string unescaped)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlNode node = doc.CreateElement("root");
                node.InnerText = unescaped;
                return node.InnerXml;
            }
            catch
            {
                return unescaped;
            }
        }

        private string[] GetPathToScenarioById(string id, bool withInfo = true)
        {
            string[] pathToScenarioRes = new string[0];
            string path = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());
            if (!Directory.Exists(path))
                return pathToScenarioRes;

            string[] idSplit = id.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            string scenarioPath = ApiHelper.FindFolderById(idSplit[idSplit.Length - 1], PathHelper.GetRepoPath());
            if (!string.IsNullOrEmpty(scenarioPath))
            {
                pathToScenarioRes = new string[4];
                idSplit = scenarioPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (withInfo)
                {
                    pathToScenarioRes[0] = Path.Combine(scenarioPath, new DirectoryInfo(scenarioPath).Name + ".info");
                }
                else
                {
                    pathToScenarioRes[0] = scenarioPath;
                }
                pathToScenarioRes[1] = idSplit[idSplit.Length - 3];
                pathToScenarioRes[2] = idSplit[idSplit.Length - 2];
                pathToScenarioRes[3] = idSplit[idSplit.Length - 1];
            }

            return pathToScenarioRes;
        }
    }
}