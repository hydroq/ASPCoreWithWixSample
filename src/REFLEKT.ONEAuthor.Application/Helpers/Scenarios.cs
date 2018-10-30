using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Authorization;
using REFLEKT.ONEAuthor.Application.Models;

namespace REFLEKT.ONEAuthor.Application.Helpers
{
    public class Scenarios
    {
        private readonly IAuthenticationManager _authenticationManager;

        public Scenarios(IAuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }

        public List<Category> GenerateJsonList(string path, string token, bool isDraft, string host)
        {
            var userName = _authenticationManager.GetActiveUserByToken(token);

            var currentUrl = HttpUtility.UrlEncode(host);
            List<Category> scenarios = new List<Category>();

            string json = "{";
            foreach (string category in Directory.GetDirectories(path))
            {
                if (File.Exists(Path.Combine(category, new DirectoryInfo(category).Name + ".info")))
                {
                    string catPath = Path.Combine(category, new DirectoryInfo(category).Name + ".info");
                    Category cat = new Category();
                    cat.products = new List<Product>();
                    cat.name = JsonConvert.DeserializeObject<PublishedScenario>(File.ReadAllText(catPath)).title;
                    cat.id = new DirectoryInfo(category).Name.Split(new char[] { '.' })[0];

                    long writeTimeCat = ConvertToUnixTimestamp(File.GetLastWriteTimeUtc(catPath));
                    long creationTimeCat = ConvertToUnixTimestamp(File.GetCreationTimeUtc(catPath));

                    if (writeTimeCat > creationTimeCat)
                    {
                        cat.modified_utc = writeTimeCat;
                    }
                    else
                    {
                        cat.modified_utc = creationTimeCat;
                    }

                    foreach (string product in Directory.GetDirectories(category))
                    {

                        if (File.Exists(Path.Combine(product, new DirectoryInfo(product).Name + ".info")))
                        {
                            string prodPath = Path.Combine(product, new DirectoryInfo(product).Name + ".info");
                            Product prod = new Product();
                            prod.scenarios = new List<Scenario>();
                            prod.name = JsonConvert.DeserializeObject<PublishedScenario>(File.ReadAllText(prodPath)).title;
                            prod.id = new DirectoryInfo(product).Name.Split(new char[] { '.' })[0];

                            long writeTimeProd = ConvertToUnixTimestamp(File.GetLastWriteTimeUtc(prodPath));
                            long creationTimeProd = ConvertToUnixTimestamp(File.GetCreationTimeUtc(prodPath));

                            if (writeTimeProd > creationTimeProd)
                            {
                                prod.modified_utc = writeTimeProd;
                            }
                            else
                            {
                                prod.modified_utc = creationTimeProd;
                            }

                            foreach (string scenario in Directory.GetDirectories(product))
                            {
                                string folder = Path.Combine(scenario, "topics");

                                if (!Directory.Exists(folder))
                                {
                                    Directory.CreateDirectory(folder);
                                    Directory.CreateDirectory(Path.Combine(scenario, "default"));
                                    Directory.CreateDirectory(Path.Combine(scenario, "default/pub_in"));
                                    Directory.CreateDirectory(Path.Combine(scenario, "default/pub_out"));
                                    Directory.CreateDirectory(Path.Combine(scenario, "default/pub_draft"));
                                    Directory.CreateDirectory(Path.Combine(scenario, "default/pub_offline"));
                                    Directory.CreateDirectory(Path.Combine(scenario, "default/projects"));
                                }

                                string pathToScenario = Path.Combine(scenario, new DirectoryInfo(scenario).Name + ".info");
                                string ditaPath = Path.Combine(scenario, "default/pub_out/" + new DirectoryInfo(category).Name
                                                           + "_" + new DirectoryInfo(product).Name +
                                                           "_" + new DirectoryInfo(scenario).Name + ".xml");

                                string draftPath = "";
                                if (isDraft)
                                {
                                    draftPath = Path.Combine(scenario, "default/pub_draft/" + new DirectoryInfo(category).Name
                                                           + "_" + new DirectoryInfo(product).Name +
                                                           "_" + new DirectoryInfo(scenario).Name + ".xml");
                                }
                                if (File.Exists(pathToScenario) || (!string.IsNullOrEmpty(draftPath) && File.Exists(draftPath)))
                                {
                                    PublishedScenario pubScenario = JsonConvert.DeserializeObject<PublishedScenario>(File.ReadAllText(pathToScenario));
                                    Scenario scen = new Scenario();


                                    long writeTime = ConvertToUnixTimestamp(File.GetLastWriteTimeUtc(pathToScenario));
                                    long creationTime = ConvertToUnixTimestamp(File.GetCreationTimeUtc(pathToScenario));
                                    //if(pubScenario != null && !string.IsNullOrEmpty(pubScenario.title))
                                    scen.name = pubScenario.title;

                                    if (writeTime > creationTime)
                                    {
                                        scen.modified_utc = writeTime;
                                    }
                                    else
                                    {
                                        scen.modified_utc = creationTime;
                                    }

                                    string dita = new DirectoryInfo(category).Name + "_" + new DirectoryInfo(product).Name + "_" + new DirectoryInfo(scenario).Name;
                                    if (File.Exists(ditaPath))
                                    {
                                        scen.uri = "ars://dita/" + dita + "?provider=xmlcompass&host=" + currentUrl + "&user=" + userName + "&token=" + token + "&dataLang=en_US&uiLang=en_US&returnurl=" + currentUrl;
                                        scen.uri = HttpUtility.UrlEncode(scen.uri);
                                    }
                                    else
                                    {
                                        scen.uri = string.Empty;
                                    }

                                    scen.id = new DirectoryInfo(scenario).Name.Split(new char[] { '.' })[0];
                                    scen.draft = string.Empty;
                                    if (isDraft && File.Exists(draftPath))
                                    {
                                        scen.draft = "ars://dita/" + dita + "_draft" + "?provider=xmlcompass&host=" + currentUrl + "&user=" + userName + "&token=" + token + "&dataLang=en_US&uiLang=en_US&returnurl=" + currentUrl;
                                        scen.draft = HttpUtility.UrlEncode(scen.draft);
                                    }

                                    PublishedScenario scenarioObj = JsonConvert.DeserializeObject<PublishedScenario>(File.ReadAllText(pathToScenario));
                                    if (scenarioObj != null && !string.IsNullOrEmpty(scenarioObj.desc))
                                        scen.description = SetImageLinks(scenarioObj.desc);
                                    prod.scenarios.Add(scen);
                                }
                            }

                            cat.products.Add(prod);
                        }

                    }
                    scenarios.Add(cat);
                }
            }
            return scenarios;
        }

        public static string SetImageLinks(String inputHTML)
        {
            const string pattern = @"<img\b[^\<\>]+?\bsrc\s*=\s*[""'](?<L>.+?)[""'][^\<\>]*?\>";
            if (string.IsNullOrEmpty(inputHTML))
                return string.Empty;
            foreach (Match match in Regex.Matches(inputHTML, pattern, RegexOptions.IgnoreCase))
            {
                var imageLink = match.Groups["L"].Value;
                if (File.Exists(imageLink))
                {
                    string newPath = Path.Combine(PathHelper.GetViewerImagesFolder(), Path.GetFileName(imageLink));
                    File.Copy(imageLink, newPath, true);
                }
                inputHTML = inputHTML.Replace(imageLink, "images/" + Path.GetFileName(imageLink));
            }

            return inputHTML;
        }


        public string GenerateJsonString(string path, string token, bool isDraft, string host)
        {
            var userName = _authenticationManager.GetActiveUserByToken(token);

            string currentUrl = HttpUtility.UrlEncode(host);
            List<Category> scenarios = new List<Category>();

            string json = "{";
            foreach (string category in Directory.GetDirectories(path))
            {
                if (File.Exists(Path.Combine(category, new DirectoryInfo(category).Name + ".info")))
                {
                    Category cat = new Category();
                    cat.products = new List<Product>();
                    cat.name = JsonConvert.DeserializeObject<PublishedScenario>(File.ReadAllText(Path.Combine(category, new DirectoryInfo(category).Name + ".info"))).title;
                    foreach (string product in Directory.GetDirectories(category))
                    {
                        if (File.Exists(Path.Combine(product, new DirectoryInfo(product).Name + ".info")))
                        {
                            Product prod = new Product();
                            prod.scenarios = new List<Scenario>();
                            prod.name = JsonConvert.DeserializeObject<PublishedScenario>(File.ReadAllText(Path.Combine(product, new DirectoryInfo(product).Name + ".info"))).title;

                            foreach (string scenario in Directory.GetDirectories(product))
                            {
                                string pathToScenario = Path.Combine(scenario, new DirectoryInfo(scenario).Name + ".info");
                                string ditaPath = Path.Combine(scenario, "default/pub_out/" + new DirectoryInfo(category).Name
                                                           + "_" + new DirectoryInfo(product).Name +
                                                           "_" + new DirectoryInfo(scenario).Name + ".xml");

                                string draftPath = "";
                                if (isDraft)
                                {
                                    draftPath = Path.Combine(scenario, "default/pub_draft/" + new DirectoryInfo(category).Name
                                                           + "_" + new DirectoryInfo(product).Name +
                                                           "_" + new DirectoryInfo(scenario).Name + ".xml");
                                }
                                if (File.Exists(pathToScenario) && (File.Exists(ditaPath) || (!string.IsNullOrEmpty(draftPath) && File.Exists(draftPath))))
                                {
                                    PublishedScenario pubScenario = JsonConvert.DeserializeObject<PublishedScenario>(File.ReadAllText(pathToScenario));
                                    Scenario scen = new Scenario();

                                    scen.name = pubScenario.title;
                                    string dita = new DirectoryInfo(category).Name + "_" + new DirectoryInfo(product).Name + "_" + new DirectoryInfo(scenario).Name;
                                    if (File.Exists(ditaPath))
                                    {
                                        scen.uri = "ars://dita/" + dita + "?provider=xmlcompass&host=" + currentUrl + "&user=" + userName + "&token=" + token + "&dataLang=en_US&uiLang=en_US&returnurl=" + currentUrl;
                                        scen.uri = HttpUtility.UrlEncode(scen.uri);
                                    }
                                    else
                                    {
                                        scen.uri = string.Empty;
                                    }

                                    scen.id = dita;
                                    scen.draft = string.Empty;
                                    if (isDraft && File.Exists(draftPath))
                                    {
                                        scen.draft = "ars://dita/" + dita + "_draft" + "?provider=xmlcompass&host=" + currentUrl + "&user=" + userName + "&token=" + token + "&dataLang=en_US&uiLang=en_US&returnurl=" + currentUrl;
                                        scen.draft = HttpUtility.UrlEncode(scen.draft);
                                    }
                                    prod.scenarios.Add(scen);
                                }
                            }
                            cat.products.Add(prod);
                        }
                    }
                    scenarios.Add(cat);
                }
            }

            foreach (Category cat in scenarios)
            {
                json += "\"" + cat.name + "\" : ";
                if (cat.products.Count == 0)
                {
                    json += "[],";
                }
                else
                {
                    json += "{ ";
                    foreach (Product prod in cat.products)
                    {
                        json += "\"" + prod.name + "\" : [";
                        if (prod.scenarios.Count > 0)
                        {
                            foreach (Scenario sce in prod.scenarios)
                            {
                                json += "{ ";

                                if (!string.IsNullOrEmpty(sce.draft) && !string.IsNullOrEmpty(sce.uri))
                                    json += string.Format("\"id\": \"{0}\", \"name\":\"{1}\",\"uri\":\"{2}\",\"draft\":\"{3}\"", sce.id, sce.name, sce.uri.Replace("/", "\\/"), sce.draft.Replace("/", "\\/"));
                                else if (string.IsNullOrEmpty(sce.draft) && !string.IsNullOrEmpty(sce.uri))
                                    json += string.Format("\"id\": \"{0}\", \"name\":\"{1}\",\"uri\":\"{2}\"", sce.id, sce.name, sce.uri.Replace("/", "\\/"));
                                else if (!string.IsNullOrEmpty(sce.draft) && string.IsNullOrEmpty(sce.uri))
                                    json += string.Format("\"id\": \"{0}\", \"name\":\"{1}\",\"draft\":\"{2}\"", sce.id, sce.name, sce.draft.Replace("/", "\\/"));

                                json += "},";
                            }
                            json = json.Remove(json.Length - 1);
                        }
                        json += "],";
                    }
                    json = json.Remove(json.Length - 1);
                    json += "},";
                }
            }
            json = json.Remove(json.Length - 1);

            if (string.IsNullOrEmpty(json)) json = "{}";
            else json += "}";

            json = json.Replace(",}", "}");
            json = json.Replace(",]", "]");

            return json;
        }
    }
}
