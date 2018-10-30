using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Web;
using IOFile = System.IO.File;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class ArsLinksController : Controller
    {
        [Route("viewer/ars_links")]
        public string GetArsLinks()
        {
            string res = "";
            UserModel user = UsersHelper.GetRandomUser();
            string currentUrl = $"{Request.Scheme}://{Request.Host.Value}";
            currentUrl = HttpUtility.UrlEncode(currentUrl);
            List<Category> scenarios = new List<Category>();
            string path = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());

            if (!Directory.Exists(path))
            {
                return "{ }";
            }

            foreach (string category in Directory.GetDirectories(path))
            {
                if (IOFile.Exists(Path.Combine(category, new DirectoryInfo(category).Name + ".info")))
                {
                    Category cat = new Category();
                    cat.products = new List<Product>();
                    cat.name = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(Path.Combine(category, new DirectoryInfo(category).Name + ".info"))).title;
                    foreach (string product in Directory.GetDirectories(category))
                    {
                        if (IOFile.Exists(Path.Combine(product, new DirectoryInfo(product).Name + ".info")))
                        {
                            Product prod = new Product();
                            prod.scenarios = new List<Scenario>();
                            prod.name = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(Path.Combine(product, new DirectoryInfo(product).Name + ".info"))).title;

                            foreach (string scenario in Directory.GetDirectories(product))
                            {
                                string pathToScenario = Path.Combine(scenario, new DirectoryInfo(scenario).Name + ".info");
                                string ditaPath = Path.Combine(scenario, "default/pub_out/" + new DirectoryInfo(category).Name
                                                           + "_" + new DirectoryInfo(product).Name +
                                                           "_" + new DirectoryInfo(scenario).Name + ".xml");

                                if (IOFile.Exists(pathToScenario) && IOFile.Exists(ditaPath))
                                {
                                    PublishedScenario pubScenario = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(pathToScenario));
                                    Scenario scen = new Scenario();
                                    scen.name = new DirectoryInfo(scenario).Name;
                                    CategoryModel scenModel = JsonConvert.DeserializeObject<CategoryModel>(IOFile.ReadAllText(pathToScenario));
                                    string dita = new DirectoryInfo(category).Name + "_" + new DirectoryInfo(product).Name + "_" + new DirectoryInfo(scenario).Name;
                                    scen.uri = "ars://dita/" + dita + "?provider=xmlcompass&host=" + currentUrl.ToUpper() + "&user=" + user.User + "&token=" + user.StaticTicket + "&dataLang=en_US&uiLang=en_US&returnurl=" + currentUrl;
                                    scen.id = dita;
                                    prod.scenarios.Add(scen);
                                    res += "<a href=\"" + scen.uri.Replace("main/", "main") + "\"> " + scenModel.title + " </a><br>";
                                }
                            }
                            cat.products.Add(prod);
                        }
                    }
                    scenarios.Add(cat);
                }
            }

            return res;
        }
    }
}