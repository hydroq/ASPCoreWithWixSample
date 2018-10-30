using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using REFLEKT.ONEAuthor.Application.Authorization;
using REFLEKT.ONEAuthor.Application.Scenarios;
using REFLEKT.ONEAuthor.Application.Utilities;
using REFLEKT.ONEAuthor.WebAPI.Common;
using REFLEKT.ONEAuthor.WebAPI.Controllers.Base;
using IOFile = System.IO.File;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class ScenariosController : ReflektBaseController
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializerUtility _jsonSerializer;
        private readonly LocaleConverter _localeConverter;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly Scenarios _scenarios;
        private readonly IScenarioService _scenarioService;

        public ScenariosController(ILogger<ScenariosController> logger, 
            IJsonSerializerUtility jsonSerializer, 
            LocaleConverter localeConverter, 
            IAuthenticationManager authenticationManager,
            Scenarios scenarios,
            IScenarioService scenarioService)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _localeConverter = localeConverter;
            _authenticationManager = authenticationManager;
            _scenarios = scenarios;
            _scenarioService = scenarioService;
        }

        [HttpPost]
        [Route("api/scenarios/import")]
        public ImportedScenarioModel Import([FromBody] ApiFormInput formData)
        {
            ImportedScenarioModel model = new ImportedScenarioModel();

            if (!formData.local)
            {
                string fileName = formData.filename;
                string base64 = formData.file;
                string webRootPath = Path.Combine(PathHelper.GetViewerImagesFolder(), fileName);
                formData.file = webRootPath;
                byte[] fileContent = ApiHelper.DecodeBase64(base64);
                IOFile.WriteAllBytes(formData.file, fileContent);
            }

            if (string.IsNullOrEmpty(formData.file))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "File is required" });
                return model;
            }
            
            formData.file = formData.file.Replace("file:///", "");

            if (!IOFile.Exists(formData.file))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "file not exist" });
                return model;
            }

            Exporter.Import(formData.file, "", formData.overRide, (res) =>
            {
                model = res;
            });

            SVNManager.SaveChanges();

            return model;
        }

        [HttpPost]
        [Route("api/scenarios/file/import")]
        public LinkModel UploadFileToServer([FromBody] ApiFormInput formData)
        {
            LinkModel model = new LinkModel();
            formData.filename = formData.filename.Replace("+", "_");
            if (!formData.local)
            {
                string fileName = formData.filename;
                string base64 = formData.file;
                formData.file = Path.Combine(PathHelper.GetViewerImagesFolder(), fileName);
                byte[] fileContent = ApiHelper.DecodeBase64(base64);
                IOFile.WriteAllBytes(formData.file, fileContent);
            }
            else
            {
                string fileName = formData.filename;
                IOFile.Copy(formData.file, Path.Combine(PathHelper.GetViewerImagesFolder(), fileName), true);
            }

            var currentUrl = $"{Request.Scheme}://{Request.Host.Value}/viewer/images/{formData.filename}"; // TODO: Test

            if (PathHelper.CheckIfFileIsOfImageType(formData.filename))
            {
                model.link = $"<img class=\"Default\" src=\"{currentUrl}\" />";
            }
            else
            {
                model.link = "<a href=\"" + currentUrl + "\" download>" + formData.filename + "</a>";
            }

            return model;
        }

        [HttpPost]
        [Route("api/scenarios/{scenario_id}")]
        public ScenarioModel ImportSingle(string scenario_id, [FromBody] ApiFormInput formData)
        {
            ScenarioModel model = new ScenarioModel();

            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (string.IsNullOrEmpty(formData.file) || string.IsNullOrEmpty(formData.ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }

            formData.file = formData.file.Replace("file:///", "");
            string ret = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (ret == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }
            if (!IOFile.Exists(formData.file))
            {
                //return ApiHelper.JsonError(400, new string[] { "file not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "file not exist" });
                return model;
            }

            if (!string.IsNullOrEmpty(ret))
            {
                Exporter.ImportTopic(formData.file, ret, false, (res) =>
                {
                    model = ApiHelper.GetScenarioInfoModel(scenario_id, ret);
                });
            }

            SVNManager.SaveChanges();

            return model;
        }
        
        [HttpGet]
        [Authorize]
        [Route("api/scenarios")]
        public ScenariosModel ScenariosList(string ticket = "")
        {
            var model = new ScenariosModel
            {
                scenarios = _scenarios.GenerateJsonList(PathHelper.GetRepoPath(), ticket,
                    UsersHelper.CheckUserDraft(ticket), $"{Request.Scheme}://{Request.Host.Value}")
            };

            model.scenarios = model.scenarios.OrderBy((s) => s.name).ToList();
            foreach (Category cat in model.scenarios)
            {
                if (cat.products != null)
                {
                    cat.products = cat.products.OrderBy((p) => p.name).ToList();
                    foreach (Product prod in cat.products)
                    {
                        if (prod.scenarios != null)
                            prod.scenarios = prod.scenarios.OrderBy(sc => sc.name).ToList();
                    }
                }
            }

            ScenariosModel result = model;

            return result;
        }
        
        [HttpGet]
        [Authorize]
        [Route("api/scenarios/{scenario_id}")]
        public ScenarioModel ScenarioInfo(string scenario_id)
        {
            ScenarioModel model = new ScenarioModel();
            
            string ret = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (ret == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            model = ApiHelper.GetScenarioInfoModel(scenario_id, ret);
            return model;
        }

        [HttpPost]
        [Route("api/scenarios")]
        public ScenarioModel AddScenario([FromBody] ApiFormInput formData)
        {
            ScenarioModel model = new ScenarioModel();

            if (formData == null)
            {
                model.errors = ApiHelper.JsonError(400, new [] { "error" });
                return model;
            }

            if (string.IsNullOrWhiteSpace(formData.name))
            {
                model.errors = ApiHelper.JsonError(400, new [] { "Incorrect name" });
                return model;
            }
            
            if (string.IsNullOrWhiteSpace(formData.folder_id) || string.IsNullOrWhiteSpace(formData.description) || string.IsNullOrWhiteSpace(formData.ticket))
            {
                model.errors = ApiHelper.JsonError(400, new [] { "error" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }
            
            model = ApiHelper.CreateScenario(formData.folder_id, formData.name, formData.description);

            SVNManager.SaveChanges();

            return model;
        }

        [HttpDelete]
        [Authorize]
        [Route("api/scenarios/{scenarioId}")]
        public ScenarioIdModel DeleteScenario(string scenarioId)
        {
            string scenarioFolder = ApiHelper.FindFolderById(scenarioId, PathHelper.GetRepoPath());
            if (string.IsNullOrWhiteSpace(scenarioFolder))
            {
                return CustomBadRequestResult<ScenarioIdModel>("Specified scenario was not found");
            }

            if (_scenarioService.CheckIfScenarioIsBusy(scenarioId))
            {
                return CustomBadRequestResult<ScenarioIdModel>("Scenario is busy at the moment");
            }
            
            Command.RemoveFolder(scenarioFolder);
            SVNManager.SaveChanges();

            return new ScenarioIdModel { scenario_id = scenarioId };
        }

        [HttpPatch]
        [Route("api/scenarios/{scenario_id}")]
        public ScenarioIdModel UpdateScenario(string scenario_id, [FromBody] ApiFormInput formData)
        {
            ScenarioIdModel result = new ScenarioIdModel();

            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                result.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return result;
            }

            if (string.IsNullOrEmpty(formData.ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                result.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return result;
            }

            if (string.IsNullOrEmpty(formData.folder.name))
            {
                result.errors = ApiHelper.JsonError(400, new string[] { "Folder name are empty" });
                return result;
            }

            if (string.IsNullOrEmpty(formData.folder.name))
            {
                result.errors = ApiHelper.JsonError(400, new string[] { "Incorrect name" });
                return result;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                result.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return result;
            }

            string ret = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (ret == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                result.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return result;
            }

            if (formData.folder == null)
            {
                if (formData.name.Length > 50)
                {
                    result.errors = ApiHelper.JsonError(400, new string[] { "Name are too long" });
                    return result;
                }
                
                try
                {
                    string folder = new DirectoryInfo(ret).Name;
                    DescModel model = JsonConvert.DeserializeObject<DescModel>(IOFile.ReadAllText(Path.Combine(ret, folder + ".info")));
                    model.title = formData.name;
                    model.desc = formData.description;// System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(sc.description));
                    string newModel = JsonConvert.SerializeObject(model);
                    IOFile.WriteAllText(Path.Combine(ret, folder + ".info"), newModel);
                    //PathHelper.RenameFolder(ret, folder, formData.name);
                    //return "{ scenario_id : \"" + scenario_id + "\" }";
                    result.scenario_id = scenario_id;

                    SVNManager.SaveChanges();

                    return result;
                }
                catch (Exception ex)
                {
                    //return ApiHelper.JsonError(400, new string[] { "wrong scenario object" });
                    result.errors = ApiHelper.JsonError(400, new string[] { "wrong scenario object" });
                    return result;
                }
            }
            else
            {
                try
                {
                    if (formData.folder.name.Length > 50)
                    {
                        result.errors = ApiHelper.JsonError(400, new string[] { "Name are too long" });
                        return result;
                    }
                    
                    string folder = new DirectoryInfo(ret).Name;
                    DescModel model = JsonConvert.DeserializeObject<DescModel>(IOFile.ReadAllText(Path.Combine(ret, folder + ".info")));
                    model.title = formData.folder.name;
                    model.desc = formData.folder.description;// System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(sc.description));
                    string newModel = JsonConvert.SerializeObject(model);
                    IOFile.WriteAllText(Path.Combine(ret, folder + ".info"), newModel);
                    //PathHelper.RenameFolder(ret, folder, formData.folder.name);
                    //return "{ scenario_id : \"" + scenario_id + "\" }";
                    result.scenario_id = scenario_id;

                    SVNManager.SaveChanges();

                    return result;
                }
                catch (Exception ex)
                {
                    //return ApiHelper.JsonError(400, new string[] { "wrong scenario object" });
                    result.errors = ApiHelper.JsonError(400, new string[] { "wrong scenario object" });
                    return result;
                }
            }
            return null;
        }

        [HttpPatch]
        [Route("api/scenarios/{scenario_id}/topics/{topic_id}/xml/update")]
        public XmlUpdateModel UpdateXML(string scenario_id, string topic_id, [FromBody] ApiFormInput formData)
        {
            XmlUpdateModel result = new XmlUpdateModel();

            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                result.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return result;
            }

            if (string.IsNullOrEmpty(formData.ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                result.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return result;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                result.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return result;
            }

            string ret = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (ret == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                result.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return result;
            }

            if (!formData.local)
            {
                string fileName = "_42722AA8E7024D0CA1628DD4F4E8B8F7.xml";
                string base64 = formData.xml;
                formData.file = Path.Combine(PathHelper.GetViewerImagesFolder(), fileName);
                byte[] fileContent = ApiHelper.DecodeBase64(base64);
                IOFile.WriteAllBytes(formData.file, fileContent);
            }

            if (string.IsNullOrEmpty(formData.file))
            {
                result.errors = ApiHelper.JsonError(400, new string[] { "File is required" });
                return result;
            }

            try
            {
                string xmlContent = IOFile.ReadAllText(formData.file);
                string topicsFolder = Path.Combine(ret, "topics");

                foreach (string t in Directory.GetFiles(topicsFolder))
                {
                    TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));
                    if (((!formData.unique_id && topic.index.ToString() == topic_id) || (formData.unique_id && topic.unique_id == topic_id)) && !string.IsNullOrEmpty(topic.vmpPath))
                    {
                        Match match = Regex.Match(xmlContent, @"<title>(.*?)</title>");
                        if (match.Groups.Count > 0)
                        {
                            string title_name = match.Groups[1].Value;
                            topic.name = title_name;
                            IOFile.WriteAllText(t, JsonConvert.SerializeObject(topic));
                        }
                        else
                        {
                            match = Regex.Match(xmlContent, @"<title (.*?)</title>");
                            if (match.Groups.Count > 0)
                            {
                                string title_name = match.Groups[1].Value;
                                if (title_name.Contains("\">"))
                                {
                                    title_name = title_name.Split(new string[] { "\">" }, StringSplitOptions.RemoveEmptyEntries)[1];
                                }
                                topic.name = title_name;
                                IOFile.WriteAllText(t, JsonConvert.SerializeObject(topic));
                            }
                        }

                        Exporter.ReplaceFileInZip(topic.vmpPath, formData.file);
                        string vmbpath = topic.vmpPath.Replace(".vmp", ".vmb");
                        if (IOFile.Exists(vmbpath))
                            Exporter.ReplaceFileInZip(vmbpath, formData.file);
                        result.scenario_id = scenario_id;
                        result.topic_id = topic_id;
                        result.message = "Topic updated";
                        SVNManager.SaveChanges();
                        return result;
                    }
                }

                result.errors = ApiHelper.JsonError(400, new string[] { "wrong topic id" });
            }
            catch
            {
                //return ApiHelper.JsonError(400, new string[] { "wrong scenario object" });
                result.errors = ApiHelper.JsonError(400, new string[] { "wrong scenario object" });
                return result;
            }
            return null;
        }

        [HttpPost]
        [Route("api/scenarios/{scenario_id}/upload")]
        public ScenarioIdWithFilepathModel UploadFile(string scenario_id, [FromBody] ApiFormInput formData)
        {
            ScenarioIdWithFilepathModel model = new ScenarioIdWithFilepathModel();

            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (string.IsNullOrEmpty(formData.file) || string.IsNullOrEmpty(formData.ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }

            formData.file = formData.file.Replace("file:///", "");
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            if (string.IsNullOrEmpty(formData.file))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "File is required" });
                return model;
            }

            if (!IOFile.Exists(formData.file) && formData.local)
            {
                //return ApiHelper.JsonError(400, new string[] { "file not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "file not exist" });
                return model;
            }
            string startedFolder = Path.Combine(folder, "default");
            folder = Path.Combine(folder, "default", "pub_in");
            string folderOut = Path.Combine(startedFolder, "pub_out");
            string folderDraft = Path.Combine(startedFolder, "pub_draft");
            if (formData.local)
            {
                if (IOFile.Exists(formData.file))
                {
                    string filename = new FileInfo(formData.file).Name;
                    if (IOFile.Exists(Path.Combine(folder, filename)))
                    {
                        IOFile.Copy(formData.file, Path.Combine(folder, filename), true);
                        IOFile.Copy(formData.file, Path.Combine(folderOut, filename), true);
                        IOFile.Copy(formData.file, Path.Combine(folderDraft, filename), true);
                        model.scenario_id = scenario_id;
                        model.file_path = Path.Combine(folder, filename);
                        //return "{scenario_id:\"" + scenario_id + "\",file_path:\"" + Path.Combine(folder, filename) + "\"}";
                        SVNManager.SaveChanges();
                        return model;
                    }
                }
            }
            else
            {
                //ApiHelper.Base64File fl = JsonConvert.DeserializeObject<ApiHelper.Base64File>(formData.file);
                byte[] bytes = ApiHelper.DecodeBase64(formData.file);
                string filename = formData.filename;
                IOFile.WriteAllBytes(Path.Combine(folder, filename), bytes);
                IOFile.WriteAllBytes(Path.Combine(folderOut, filename), bytes);
                IOFile.WriteAllBytes(Path.Combine(folderDraft, filename), bytes);

                model.scenario_id = scenario_id;
                model.file_path = Path.Combine(folder, filename).Replace("\\", "/");
                //return "{scenario_id:\"" + scenario_id + "\",file_path:\"" + Path.Combine(folder, filename) + "\"}";
                SVNManager.SaveChanges();
                return model;
            }
            //return ApiHelper.JsonError(400, new string[] { "wrong file" });
            model.errors = ApiHelper.JsonError(400, new string[] { "wrong file" });
            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/scenarios/{scenario_id}/status")]
        public ScenarioStatusModel ScenarioStatus(string scenario_id)
        {
            ScenarioStatusModel model = new ScenarioStatusModel();
            
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.status = "saved";
                return model;
            }

            string infoPath = Path.Combine(folder, new DirectoryInfo(folder).Name + ".info");
            string topicsFolder = Path.Combine(folder, "topics");
            string projectsFolder = Path.Combine(folder, "default", "projects");
            string publishingFolder = Path.Combine(folder, "default", "pub_out");

            bool isPublishing = false;
            bool isEditing = false;

            foreach (string t in Directory.GetFiles(topicsFolder))
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));
                if (topic.isOpened != 0)
                    isEditing = true;
            }

            if (!isEditing)
            {
                DateTime contentModifDate = new DirectoryInfo(projectsFolder).LastWriteTimeUtc;
                DateTime publicationModifDate = new DirectoryInfo(publishingFolder).LastWriteTimeUtc;
                isPublishing = publicationModifDate > contentModifDate;
            }
            string infoFile = Path.Combine(folder, new DirectoryInfo(folder).Name + ".info");
            PublishedScenario scenario = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(infoFile));

            model.scenario_id = scenario_id;
            if (scenario.isPublishing)
            {
                model.status = "publishing";
            }
            else if (isEditing)
            {
                model.status = "editing";
            }
            else
            {
                model.status = "saved";
            }

            model.path_to_info = infoFile;
            model.isPub = scenario.isPublishing;

            if (string.IsNullOrEmpty(model.status))
            {
                model.status = "saved";
                model.isPub = false;
            }
            //string ret = "{scenario_id:\"" + scenario_id + "\", status:" + status + "}";
            return model;
        }

        public bool CanEditStatus(string scenario_id)
        {
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                return false;
            }

            string infoPath = Path.Combine(folder, new DirectoryInfo(folder).Name + ".info");
            string topicsFolder = Path.Combine(folder, "topics");
            string projectsFolder = Path.Combine(folder, "default", "projects");
            string publishingFolder = Path.Combine(folder, "default", "pub_out");

            bool isPublishing = false;
            bool isEditing = false;

            foreach (string t in Directory.GetFiles(topicsFolder))
            {
                string topicContent = IOFile.ReadAllText(t);
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(topicContent);
                if (topic.isOpened != 0)
                {
                    isEditing = true;

                    _logger.LogWarning($"Topic '{topic.name}' is currently opened for editing. Details: {topicContent}");
                }
            }

            if (!isEditing)
            {
                DateTime contentModifDate = new DirectoryInfo(projectsFolder).LastWriteTimeUtc;
                DateTime publicationModifDate = new DirectoryInfo(publishingFolder).LastWriteTimeUtc;

                isPublishing = publicationModifDate > contentModifDate;

                if (isPublishing)
                {
                    _logger.LogWarning($"/default/projects mofification date ({contentModifDate}) IS BIGGER THAN /default/pub_out ({publicationModifDate})");
                }
            }

            string infoFile = Path.Combine(folder, new DirectoryInfo(folder).Name + ".info");
            PublishedScenario scenario = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(infoFile));

            if (scenario.isPublishing)
            {
                _logger.LogWarning($"Scenario '{scenario.id}' is already publishing");

                return false;
            }
            else if (isEditing)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/scenarios/{scenario_id}/publication_status")]
        public ScenarioPublicationStatusModel ScenarioPublicationStatus(string scenario_id)
        {
            ScenarioPublicationStatusModel model = new ScenarioPublicationStatusModel();
            
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            string contentPath = Path.Combine(folder, "default", "projects");
            string draftPath = Path.Combine(folder, "default", "pub_draft");
            string publicationPath = Path.Combine(folder, "default", "pub_out");

            DateTime contentModifDate = new DirectoryInfo(contentPath).LastWriteTimeUtc;
            DateTime publicationModifDate = new DirectoryInfo(publicationPath).LastWriteTimeUtc;

            model.changes = contentModifDate > publicationModifDate;
            model.draft = Directory.GetFiles(draftPath).Length > 0;
            model.publication = Directory.GetFiles(publicationPath).Length > 0;

            //return "{changes:\"" + changes + "\",draft:\"" + draft + "\",publication:\"" + publication + "\"}";
            return model;
        }

        [HttpPost]
        [Route("api/scenarios/{scenario_id}/draft")]
        public StatusModel ScenarioDraft(string scenario_id, [FromBody] ApiFormInput formData)
        {
            if (formData == null)
            {
                return CustomBadRequestResult<StatusModel>(Constants.InputDataIsMissing);
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                return CustomBadRequestResult<StatusModel>(Constants.NotAuthorized);
            }

            string ret = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());
            if (ret == null)
            {
                return CustomBadRequestResult<StatusModel>(Constants.ScenarioNotFound);
            }

            if (!CanEditStatus(scenario_id))
            {
                return CustomBadRequestResult<StatusModel>(Constants.ScenarioIsPublishingOrDraft);
            }

            StatusModel model = new StatusModel();
            string pathToIn = Path.Combine(ret, "default/pub_in");
            string pathToOut = Path.Combine(ret, "default/pub_draft");
            string infoFile = Path.Combine(ret, new DirectoryInfo(ret).Name + ".info");

            PublishedScenario scenario = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(infoFile));
            scenario.isPublishing = true;
            IOFile.WriteAllText(infoFile, JsonConvert.SerializeObject(scenario));
            Publishing.Draft(ret, (res) =>
            {
                model.status = 200;

                scenario.isPublishing = false;
                IOFile.WriteAllText(infoFile, JsonConvert.SerializeObject(scenario));

                foreach (string s in Directory.GetFiles(pathToIn))
                {
                    string fileName = Path.GetFileName(s);
                    if (!fileName.Contains(".zip") && !fileName.Contains(".rar") && !fileName.Contains(".xml"))
                        IOFile.Copy(s, Path.Combine(pathToOut, fileName), true);
                }

                SVNManager.SaveChanges();
            });
            return model;
        }

        [HttpPost]
        [Route("api/scenarios/{scenario_id}/publish")]
        public StatusModel ScenarioPublish(string scenario_id, [FromBody] ApiFormInput formData)
        {
            StatusModel model = new StatusModel();

            if (formData == null)
            {
                return CustomBadRequestResult<StatusModel>(Constants.InputDataIsMissing);
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                return CustomBadRequestResult<StatusModel>(Constants.NotAuthorized);
            }

            string ret = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());
            if (ret == null)
            {
                return CustomBadRequestResult<StatusModel>(Constants.ScenarioNotFound);
            }

            if (!CanEditStatus(scenario_id))
            {
                return CustomBadRequestResult<StatusModel>(Constants.ScenarioIsPublishingOrDraft);
            }

            string pathToIn = Path.Combine(ret, "default/pub_in");
            string pathToOut = Path.Combine(ret, "default/pub_out");
            string infoFile = Path.Combine(ret, new DirectoryInfo(ret).Name + ".info");

            PublishedScenario scenario = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(infoFile));
            scenario.isPublishing = true;
            IOFile.WriteAllText(infoFile, JsonConvert.SerializeObject(scenario));

            Publishing.Draft(ret, (res) =>
            {
                Publishing.Publish(ret, (ress) =>
                {
                    model.status = 200;
                    scenario.isPublishing = false;
                    IOFile.WriteAllText(infoFile, JsonConvert.SerializeObject(scenario));

                    foreach (string s in Directory.GetFiles(pathToIn))
                    {
                        string fileName = Path.GetFileName(s);
                        if (!fileName.Contains(".zip") && !fileName.Contains(".rar") && !fileName.Contains(".xml"))
                            IOFile.Copy(s, Path.Combine(pathToOut, fileName), true);
                    }

                    SVNManager.SaveChanges();
                });
            });

            return model;
        }

        [HttpPost]
        [Route("api/scenarios/{scenario_id}/topics")]
        public TopicReturnModel CreateTopic(string scenario_id, [FromBody] ApiFormInput formData)
        {
            TopicReturnModel result = new TopicReturnModel();

            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                result.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return result;
            }

            if (string.IsNullOrEmpty(formData.type) || string.IsNullOrEmpty(formData.ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                result.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return result;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                result.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return result;
            }

            formData.type = formData.type;

            if (!string.IsNullOrEmpty(formData.tracking_configuration))
            {
                formData.tracking_configuration = formData.tracking_configuration.Replace("file:///", "");
            }

            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                result.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return result;
            }

            string topicsPath = Path.Combine(folder, "topics");

            List<TopicObject> topics = ApiHelper.GetTopics(scenario_id);
            int poiCount = 0;
            int infoCount = 0;
            int animCount = 0;
            foreach (TopicObject t in topics)
            {
                if (t.type == "info")
                    infoCount++;
                if (t.type == "poi")
                    poiCount++;
                if (t.type == "animation")
                    animCount++;
            }

            string name = "";
            for (int i = 4 - topics.Count.ToString().Length; i > 0; i--)
            {
                name += "0";
            }

            TopicModel model = new TopicModel();
            model.type = formData.type;
            model.localization = "default";
            model.index = topics.Count;
            model.unique_id = PathHelper.GenerateId(6);
            if (!string.IsNullOrEmpty(formData.tracking_configuration))
            {
                if (IOFile.Exists(formData.tracking_configuration))
                {
                    string pathToZip = formData.tracking_configuration;
                    string pubIn = Path.Combine(folder, "default", "pub_in");
                    string pubinZip = Path.Combine(pubIn, Path.GetFileName(pathToZip).Replace(" ", ""));

                    IOFile.Copy(pathToZip, pubinZip, true);
                    model.pathToZip = pubinZip;
                }
            }

            switch (formData.type)
            {
                case "info":
                    model.name = "Information" + name + infoCount;
                    break;

                case "poi":
                    model.name = "Poitopic" + name + poiCount;
                    break;

                case "animation":
                    model.name = "Animation" + name + animCount;
                    break;
            }

            string filePath = Path.Combine(topicsPath, model.name.Replace(" ", "") + ".info");
            string vmpPath = filePath.Replace("topics", Path.Combine("default", "projects"));
            vmpPath = vmpPath.Replace(".info", ".vmp");
            vmpPath = vmpPath.Replace(Path.GetFileName(vmpPath), "") + model.type + "" + PathHelper.GenerateIntId(8) + ".vmp";
            vmpPath = vmpPath.Replace("\\", "/");
            model.vmpPath = vmpPath;

            string toolsPath = PathHelper.GetUserProcessingFolder();

            string templateZipPath = Path.Combine(PathHelper.GetToolsPath(), "temp.vmp");

            string unzipPath = Path.Combine(toolsPath, "unzip");
            string zipperFile = Path.Combine(unzipPath, model.type.Replace(" ", "_") + PathHelper.GenerateIntId(8) + ".vmp");
            string projectsFoled = Path.Combine(folder, "default", "projects");

            string finalFile = model.vmpPath;

            Directory.CreateDirectory(unzipPath);

            try
            {
                foreach (string s in Directory.GetFiles(unzipPath))
                    IOFile.Delete(s);
            }
            catch { }

            ZipFile.ExtractToDirectory(templateZipPath, unzipPath, true);

            string xmlFilePath = Path.Combine(unzipPath, "_42722AA8E7024D0CA1628DD4F4E8B8F7.xml");
            string file = IOFile.ReadAllText(xmlFilePath);
            if (model.type == "poi")
            {
                file = "<?xml version=\"1.0\"?><!DOCTYPE poitopic PUBLIC \"-//Bosch//DTD DITA AR Topic//EN\" \"artopic.dtd\"><poitopic id=\"r54311daa-6471-4634-a5e6-155dfb438d88\"><title>topicname</title><poibody><poi><title><?xm-replace_text topicname?></title></poi></poibody><poi-links>POILINKS</poi-links></poitopic>";
                file = file.Replace("x3dfilename", Path.GetFileName(finalFile).Replace(".vmp", ".x3d"));
            }
            else if (model.type == "info")
            {
                file = "<?xml version=\"1.0\"?><!DOCTYPE poitopic PUBLIC \"-//Bosch//DTD DITA AR Topic//EN\" \"artopic.dtd\"><information id=\"r54311daa-6471-4634-a5e6-ca419f591c6e\"><title>Information0002</title><infobody><infobox><title><?xm-replace_text Information0002?></title><note><?xm-replace_text Information0002?></note></infobox></infobody></information>";
            }
            else if (model.type == "animation")
            {
                file = "<?xml version=\"1.0\"?><!DOCTYPE poitopic PUBLIC \"-//Bosch//DTD DITA AR Topic//EN\" \"artopic.dtd\"><animated-procedure id=\"r54311daa-6471-4634-a5e6-155dfb438d88\"><title>topicname</title><taskbody><context></context><steps><step><cmd></cmd></step></steps></taskbody><animation-links>ANIMLINKS</animation-links></animated-procedure>";
            }
            file = file.Replace("topicname", model.name);
            char[] chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            string newString = "";
            Random random = new Random();
            for (int i = 0; i < 12; i++)
            {
                newString += chars[random.Next(0, chars.Length)];
            }
            file = file.Replace("155dfb438d88", newString);

            file = file.Replace("< ", "<");
            file = file.Replace(" >", ">");
            file = file.Replace(" />", ">");
            file = file.Replace("</ ", "</");

            IOFile.WriteAllText(xmlFilePath, file, System.Text.Encoding.UTF8);

            string[] files = new string[] { Path.Combine(unzipPath,"_42722AA8E7024D0CA1628DD4F4E8B8F7.xml"),
                Path.Combine(unzipPath, "_E14677F1E56046F9B991FF84F9D40393.xml"),
                Path.Combine(unzipPath,"Manuals.xml")};

            string path = Path.GetDirectoryName(zipperFile);
            Directory.CreateDirectory(path);

            using (var archive = ZipFile.Open(zipperFile, ZipArchiveMode.Create))
            {
                foreach (string fileName in files)
                {
                    archive.CreateEntryFromFile(fileName, Path.GetFileName(fileName));
                }
            }

            if (!Directory.Exists(projectsFoled))
            {
                Directory.CreateDirectory(projectsFoled);
            }
            IOFile.Copy(zipperFile, finalFile, true);

            IOFile.WriteAllText(filePath, JsonConvert.SerializeObject(model));

            SVNManager.SaveChanges();
            return GetTopic(scenario_id, model.index.ToString());
        }

        [HttpGet]
        [Authorize]
        [Route("api/scenarios/{scenarioId}/topics")]
        public TopicsModel GetAllTopics(string scenarioId)
        {
            var model = new TopicsModel();
            var folder = ApiHelper.FindFolderById(scenarioId, PathHelper.GetRepoPath());
            if (folder == null)
            {
                model.errors = ApiHelper.JsonError(400, new [] { "Scenario not found" });

                return model;
            }

            model.topics = ApiHelper.GetTopics(scenarioId).OrderBy(t => t.id).ToList();

            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/scenarios/{scenario_id}/topics/{topic_id}")]
        public TopicReturnModel GetTopic(string scenario_id, string topic_id, bool unique_id = false)
        {
            TopicReturnModel model = new TopicReturnModel();
            
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            List<TopicObject> topics = ApiHelper.GetTopics(scenario_id);
            foreach (TopicObject t in topics)
            {
                if (((!unique_id && t.id == topic_id) || (unique_id && t.unique_id == topic_id)))
                {
                    model.topic = t;
                    return model;
                    //return "{topic:" + JsonConvert.SerializeObject(t) + "}";
                }
            }
            //return ApiHelper.JsonError(400, new string[] { "not found" });
            model.errors = ApiHelper.JsonError(400, new string[] { "not found" });
            return model;
        }

        [HttpPatch]
        [Route("api/scenarios/{scenario_id}/topics/{topic_id}")]
        public TopicReturnModel UpdateTopic(string scenario_id, string topic_id, [FromBody] ApiFormInput formData)
        {
            TopicReturnModel model = new TopicReturnModel();

            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (string.IsNullOrEmpty(formData.type) || string.IsNullOrEmpty(formData.ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }
            
            if (!formData.local)
            {
                string fileName = formData.filename;
                string base64 = formData.tracking_configuration;
                formData.tracking_configuration = Path.Combine(PathHelper.GetViewerImagesFolder(), fileName);
                byte[] fileContent = ApiHelper.DecodeBase64(base64);
                IOFile.WriteAllBytes(formData.tracking_configuration, fileContent);
            }

            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }
            if (!IOFile.Exists(formData.tracking_configuration))
            {
                //return ApiHelper.JsonError(400, new string[] { "file not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "file not exist" });
                return model;
            }

            string topicsFolder = Path.Combine(folder, "topics");
            foreach (string t in Directory.GetFiles(topicsFolder))
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));
                if (topic.index.ToString() == topic_id)
                {
                    if (!string.IsNullOrEmpty(formData.tracking_configuration))
                    {
                        if (IOFile.Exists(formData.tracking_configuration))
                        {
                            string pathToZip = formData.tracking_configuration;
                            string pubIn = Path.Combine(folder, "default", "pub_in");
                            string pubinZip = Path.Combine(pubIn, Path.GetFileName(pathToZip).Replace(" ", ""));

                            IOFile.Copy(pathToZip, pubinZip, true);
                            topic.localization = "default";
                            topic.pathToZip = pubinZip;

                            topic.type = formData.type;

                            IOFile.WriteAllText(t, JsonConvert.SerializeObject(topic));

                            SVNManager.SaveChanges();
                            return GetTopic(scenario_id, topic_id);
                        }
                    }
                    else
                    {
                        SVNManager.SaveChanges();
                        return GetTopic(scenario_id, topic_id);
                    }
                }
            }

            //return ApiHelper.JsonError(400, new string[] { "not found" });
            model.errors = ApiHelper.JsonError(400, new string[] { "not found" });
            return model;
        }

        [HttpPatch]
        [Route("api/scenarios/{scenario_id}/topics/{topic_id}/sort")]
        public TopicReturnModel SortTopic(string scenario_id, string topic_id, [FromBody] ApiFormInput formData)
        {
            TopicReturnModel model = new TopicReturnModel();

            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }

            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            string topicsFolder = Path.Combine(folder, "topics");
            foreach (string t in Directory.GetFiles(topicsFolder))
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));

                if (((!formData.unique_id && topic.index.ToString() == topic_id) || (formData.unique_id && topic.unique_id == topic_id)))
                {
                    topic.localization = "default";
                    topic.index = formData.index;
                    IOFile.WriteAllText(t, JsonConvert.SerializeObject(topic));

                    return GetTopic(scenario_id, topic.index.ToString());
                }
            }

            SVNManager.SaveChanges();

            return model;
        }

        [HttpPatch]
        [Route("api/scenarios/{scenario_id}/topics/sort")]
        public TopicSortReturnModel SortTopics(string scenario_id, [FromBody] ApiFormInput formData)
        {
            TopicSortReturnModel model = new TopicSortReturnModel();

            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }

            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }
            string topicsFolder = Path.Combine(folder, "topics");
            foreach (SortTopicModel sort in formData.topics)
            {
                foreach (string t in Directory.GetFiles(topicsFolder))
                {
                    TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));
                    if (sort.old_id == topic.index)
                    {
                        sort.path = t;
                        continue;
                    }
                }
            }

            foreach (SortTopicModel sort in formData.topics)
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(sort.path));

                topic.localization = "default";
                topic.index = sort.new_id;
                IOFile.WriteAllText(sort.path, JsonConvert.SerializeObject(topic));
            }

            //return ApiHelper.JsonError(400, new string[] { "not found" });

            model.topics = formData.topics;

            SVNManager.SaveChanges();
            return model;
        }
        
        [HttpDelete]
        [Authorize]
        [Route("api/scenarios/{scenario_id}/topics/{topic_id}")]
        public TopicIdModel DeleteTopic(string scenario_id, string topic_id, bool unique_id = false)
        {
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());
            if (folder == null)
            {
                return new TopicIdModel { errors = BadRequestJson(Constants.ScenarioNotFound) };
            }

            string topicsFolder = Path.Combine(folder, "topics");
            foreach (string t in Directory.GetFiles(topicsFolder))
            {
                var topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));
                if (topic.isOpened != 0)
                {
                    return new TopicIdModel { errors = BadRequestJson(Constants.TopicIsOpened) };
                }

                var model = new TopicIdModel();
                if (((!unique_id && topic.index.ToString() == topic_id) || (unique_id && topic.unique_id == topic_id)))
                {
                    IOFile.Delete(t);
                    model.topic_id = topic_id;
                    SVNManager.SaveChanges();
                    return model;
                }
            }

            return new TopicIdModel { errors = BadRequestJson(Constants.TopicNotFound) };
        }

        [HttpGet]
        [Authorize]
        [Route("api/scenarios/{scenario_id}/topics/{topic_id}/status")]
        public TopicStatusModel TopicStatus(string scenario_id, string topic_id, bool unique_id = false)
        {
            TopicStatusModel model = new TopicStatusModel();
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            folder = Path.Combine(folder, "topics");
            foreach (string t in Directory.GetFiles(folder))
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));
                if (((!unique_id && topic.index.ToString() == topic_id) || (unique_id && topic.unique_id == topic_id)))
                {
                    model.status = topic.isOpened != 0 ? "editing" : "saved";
                    //return "{status: \"" + status + "\"}";
                    return model;
                }
            }

            //return ApiHelper.JsonError(400, new string[] { "not found" });
            model.errors = ApiHelper.JsonError(400, new string[] { "error" });
            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/scenarios/export")]
        public FileLinkModel ExportAll()
        {
            FileLinkModel model = new FileLinkModel();
            string zipName = PathHelper.GenerateIntId(16);
            string zip = Path.Combine(PathHelper.GetUploadFolder(), zipName);

            Exporter.Export(zip, "", ExportType.All);

            model.file_url = "file:///" + zip.Replace("\\", "/") + ".zip";

            return model;
        }

        [HttpGet]
        [Authorize] 
        [Route("api/scenarios/{scenarioId}/export")]
        public FileLinkModel ExportScenario(string scenarioId)
        {
            FileLinkModel model = new FileLinkModel();
            
            string scenarioFolder = ApiHelper.FindFolderById(scenarioId, PathHelper.GetRepoPath());
            if (scenarioFolder == null)
            {
                model.errors = ApiHelper.JsonError(400, new[] { "Scenario doesn't exist" });

                return model;
            }

            var scenarioInfoFile = Path.Combine(scenarioFolder, new DirectoryInfo(scenarioFolder).Name + ".info");

            PublishedScenario scenarioModel = _jsonSerializer.DeserializeFromFile<PublishedScenario>(scenarioInfoFile);
            _localeConverter.FixPublishedScenarioTitle(scenarioModel);

            string zipName = _localeConverter.FixFileNaming(scenarioModel.title + "_" + ApiHelper.GetCurrentDate());
            string zip = Path.Combine(PathHelper.GetUploadFolder(), zipName);

            Exporter.Export(zip, scenarioInfoFile, ExportType.Scenario);

            model.file_url = "file:///" + zip.Replace("\\", "/") + ".zip";

            return model;
        }

        // TODO: Move to separate controller
        [HttpGet]
        [Route("api/scenarios/{scenario_id}/topics/{topic_id}/export")]
        public FileLinkModel ExportTopic(string scenario_id, string topic_id, bool unique_id = false)
        {
            FileLinkModel model = new FileLinkModel();
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            folder = Path.Combine(folder, "topics");

            string zipName = PathHelper.GenerateIntId(16);
            foreach (string t in Directory.GetFiles(folder))
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));
                if (((!unique_id && topic.index.ToString() == topic_id) || (unique_id && topic.unique_id == topic_id)))
                {
                    topic.name = topic.name.Replace("ü", "u");
                    topic.name = topic.name.Replace("ä ", "a");
                    topic.name = topic.name.Replace("ö", "o");

                    zipName = topic.name + "_" + ApiHelper.GetCurrentDate();
                    zipName = zipName.Replace(":", ".");
                    zipName = zipName.Replace("+", "_");
                    zipName = zipName.Replace("ä ", "a");
                    zipName = zipName.Replace("ö", "o");
                    zipName = zipName.Replace("ü", "u");
                    var zip = Path.Combine(PathHelper.GetUploadFolder(), zipName);

                    Exporter.Export(zip, t, ExportType.Topic);
                    model.file_url = "file:///" + zip.Replace("\\", "/") + ".zip";
                    return model;
                }
            }

            folder = Path.Combine(folder, new DirectoryInfo(folder).Name + ".info");

            //return "{file_url: \"" + zip + ".zip\"}";
            model.errors = ApiHelper.JsonError(400, new string[] { "topic not exist" });
            return model;
        }

        [HttpGet]
        [Route("scenarios")]
        [Route("viewer/scenarios")]
        public ActionResult DownloadScenariosAsJson(string ticket)
        {
            if (string.IsNullOrEmpty(ticket) || !_authenticationManager.CheckAccessToken(ticket, out var userName))
            {
                // Response status is required by the client application
                return new StatusCodeResult(302);
            }

            var isDraft = UsersHelper.CheckUserDraft(ticket);
            if (!SVNManager.IsLocal())
            {
                SVNManager.UpdateLocalRepo();
            }

            return GenerateJson(ticket, isDraft);

        }

        private ActionResult GenerateJson(string token, bool isDraft)
        {
            string path = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder());

            if (!Directory.Exists(path))
            {
                NotFound();
            }

            string json = GenerateJsonString(path, token, isDraft);
            string resultFilePath = Path.Combine(PathHelper.GetUserProcessingFolder(), "scenarios.json");
            if (!IOFile.Exists(resultFilePath))
            {
                IOFile.Create(resultFilePath).Dispose();
            }

            IOFile.WriteAllText(resultFilePath, json);

            return PhysicalFile(resultFilePath, "application/octet-stream", Path.GetFileName(resultFilePath));
        }

        private string GenerateJsonString(string path, string token, bool isDraft)
        {
            UserModel user = new UserModel { User = _authenticationManager.GetActiveUserByToken(token)};
            string currentUrl = $"{Request.Scheme}://{Request.Host.Value}/viewer";
            currentUrl = HttpUtility.UrlEncode(currentUrl);
            List<Category> scenarios = new List<Category>();

            string json = "{";
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

                                string draftPath = "";
                                if (isDraft)
                                {
                                    draftPath = Path.Combine(scenario, "default/pub_draft/" + new DirectoryInfo(category).Name
                                                           + "_" + new DirectoryInfo(product).Name +
                                                           "_" + new DirectoryInfo(scenario).Name + ".xml");
                                }
                                if (IOFile.Exists(pathToScenario) && (IOFile.Exists(ditaPath) || (!string.IsNullOrEmpty(draftPath) && IOFile.Exists(draftPath))))
                                {
                                    PublishedScenario pubScenario = JsonConvert.DeserializeObject<PublishedScenario>(IOFile.ReadAllText(pathToScenario));
                                    Scenario scen = new Scenario();

                                    scen.name = pubScenario.title;
                                    string dita = new DirectoryInfo(category).Name + "_" + new DirectoryInfo(product).Name + "_" + new DirectoryInfo(scenario).Name;
                                    if (IOFile.Exists(ditaPath))
                                    {
                                        scen.uri = "ars://dita/" + dita + "?provider=xmlcompass&host=" + currentUrl + "&user=" + user.User + "&token=" + token + "&dataLang=en_US&uiLang=en_US&returnurl=" + currentUrl;
                                        scen.uri = HttpUtility.UrlEncode(scen.uri);
                                    }
                                    else
                                    {
                                        scen.uri = string.Empty;
                                    }

                                    scen.id = dita;
                                    scen.draft = string.Empty;
                                    if (isDraft && IOFile.Exists(draftPath))
                                    {
                                        scen.draft = "ars://dita/" + dita + "_draft" + "?provider=xmlcompass&host=" + currentUrl + "&user=" + user.User + "&token=" + token + "&dataLang=en_US&uiLang=en_US&returnurl=" + currentUrl;
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