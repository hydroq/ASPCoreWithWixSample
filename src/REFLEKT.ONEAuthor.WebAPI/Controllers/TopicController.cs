using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application;
using REFLEKT.ONEAuthor.Application.Authorization;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using REFLEKT.ONEAuthor.Application.Topics;
using REFLEKT.ONEAuthor.WebAPI.Common;
using REFLEKT.ONEAuthor.WebAPI.Controllers.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using REFLEKT.ONEAuthor.Application.Scenarios;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class TopicController : ReflektBaseController
    {
        private readonly ILogger<TopicController> _logger;
        private readonly IScenarioService _scenarioService;
        private readonly ITopicService _topicService;
        private readonly IAuthenticationManager _authenticationManager;

        public TopicController(ILogger<TopicController> logger, IScenarioService scenarioService, ITopicService topicService, IAuthenticationManager authenticationManager)
        {
            _logger = logger;
            _scenarioService = scenarioService;
            _topicService = topicService;
            _authenticationManager = authenticationManager;
        }

        [HttpPost]
        [Route("api/scenarios/{scenarioId}/topics/{topicId}/edit")]
        public StatusModel EditTopic(string scenarioId, string topicId, [FromBody] ApiFormInput formData)
        {
            if (formData == null)
            {
                return CustomBadRequestResult<StatusModel>(Constants.InputDataIsMissing);
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                return CustomBadRequestResult<StatusModel>(Constants.NotAuthorized);
            }

            try
            {
                var scenarioFolder = _scenarioService.GetScenarioFolder(scenarioId);

                _topicService.EditTopic(scenarioFolder, formData.unique_id, topicId);
            }
            catch (Exception ex)
            {
                return CustomBadRequestResult<StatusModel>(ex.Message);
            }

            return new StatusModel();
        }

        [HttpPost]
        [Route("api/scenarios/{scenario_id}/topics/import")]
        public ImportedTopicsModel ImportTopic(string scenario_id, [FromBody] ApiFormInput formData)
        {
            ImportedTopicsModel model = new ImportedTopicsModel();

            if (!formData.local)
            {
                string fileName = formData.filename;
                string base64 = formData.file;
                formData.file = Path.Combine(PathHelper.GetViewerImagesFolder(), fileName);
                byte[] fileContent = ApiHelper.DecodeBase64(base64);
                System.IO.File.WriteAllBytes(formData.file, fileContent);
            }

            if (string.IsNullOrEmpty(formData.ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (string.IsNullOrEmpty(formData.file))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "File is required" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }

            formData.file = formData.file.Replace("file:///", "");

            if (!System.IO.File.Exists(formData.file))
            {
                //return ApiHelper.JsonError(400, new string[] { "file not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "file not exist" });
                return model;
            }
            string ret = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            List<TopicObject> beforeImportTopics = ApiHelper.GetTopics(scenario_id);

            Exporter.ImportTopic(formData.file, ret, formData.overRide, (res) =>
            {
                List<TopicObject> topics = ApiHelper.GetTopics(scenario_id);
                topics = topics.OrderBy(t => t.id).ToList();
                List<TopicModel> orogintopics = ApiHelper.GetTopicsOrigin(scenario_id);
                List<int> ids = new List<int>();
                foreach (TopicObject obj in topics)
                {
                    int index = int.Parse(obj.id);
                    while (ids.Contains(index))
                    {
                        index += 1;
                    }
                    ids.Add(index);
                    obj.id = index.ToString();
                }

                ids.Clear();

                foreach (TopicModel obj in orogintopics)
                {
                    int index = obj.index;
                    while (ids.Contains(index))
                    {
                        index += 1;
                    }
                    obj.index = index;
                    ids.Add(index);
                    System.IO.File.WriteAllText(obj.infoPath, JsonConvert.SerializeObject(obj));
                }

                foreach (TopicObject lastObj in beforeImportTopics)
                {
                    topics.Remove(topics.Find(t => t.id == lastObj.id));
                }

                topics = topics.OrderBy(t => t.id).ToList();
                model.topics = topics;
            });

            SVNManager.SaveChanges();

            return model;
        }
    }
}