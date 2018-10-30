using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using REFLEKT.ONEAuthor.Application.Scenarios;
using REFLEKT.ONEAuthor.Application.Settings;
using REFLEKT.ONEAuthor.Application.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace REFLEKT.ONEAuthor.Application.Topics
{
    public class TopicService : ITopicService
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger<TopicService> _logger;
        private readonly IJsonSerializerUtility _jsonSerializer;

        public TopicService(ISettingsManager settingsManager, ILogger<TopicService> logger, IJsonSerializerUtility jsonSerializer)
        {
            _settingsManager = settingsManager;
            _logger = logger;
            _jsonSerializer = jsonSerializer;
        }

        public void EditTopic(string scenarioFolder, bool uniqueId, string topicId)
        {
            string topicsFolder = Path.Combine(scenarioFolder, "topics");
            foreach (string t in Directory.GetFiles(topicsFolder))
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(File.ReadAllText(t));
                if (((!uniqueId && topic.index.ToString() == topicId) || (uniqueId && topic.unique_id == topicId)))
                {
                    RunCortona(topic, t, _settingsManager.GetRapidManualPath());

                    break;
                }
            }

            SVNManager.SaveChanges();
        }

        public IEnumerable<TopicModel> GetEditingTopicsFromFolder(string topicsFolderName)
        {
            return Directory.GetFiles(topicsFolderName)
                    .Select(t => _jsonSerializer.DeserializeFromFile<TopicModel>(t))
                    .Where(t => t.isOpened > 0);
        }

        private void RunCortona(TopicModel topic, string tPath, string appName)
        {
            if (string.IsNullOrEmpty(topic.vmpPath))
            {
                topic.vmpPath = topic.vmpPathDEFAULT;
            }

            topic.isOpened = 1;
            File.WriteAllText(tPath, JsonConvert.SerializeObject(topic));

            ExternalProgram.OpenAppWindow(topic, tPath, appName, PathHelper.GetToolsPath(), onExit: UpdateTopicNameOnCortonaExit);
        }

        private void UpdateTopicNameOnCortonaExit(TopicModel topic, string topicPath, string pathToVmp)
        {
            var rootDefaultProjectsDirectory = pathToVmp.Replace(Path.GetExtension(pathToVmp), "");

            try
            {
                _logger.LogDebug($"Extracting VMP to directory: {rootDefaultProjectsDirectory}");
                using (ZipArchiveUtility.ExtractToDirectory(pathToVmp, rootDefaultProjectsDirectory))
                {
                    foreach (string xmlFile in Directory.GetFiles(rootDefaultProjectsDirectory)
                                                        .Where(f => Path.GetExtension(f).Contains("xml")))
                    {
                        string fileContent = File.ReadAllText(xmlFile);
                        if (fileContent.IndexOf("<title", StringComparison.CurrentCultureIgnoreCase) == -1)
                        {
                            continue;
                        }

                        var titleRegex = Regex.Match(fileContent, @"<title(?:.*?)>(.+?)(?:<\/title>|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        topic.name = titleRegex.Groups[1].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing VMP for Topic ID: {topic.unique_id}");
            }

            topic.isOpened = 0;

            _logger.LogInformation($"Updating topic name to {topic.name} (path = {topicPath})");
            File.WriteAllText(topicPath, JsonConvert.SerializeObject(topic));
        }
    }
}