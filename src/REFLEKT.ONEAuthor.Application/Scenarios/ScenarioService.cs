using Microsoft.Extensions.Logging;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using REFLEKT.ONEAuthor.Application.Topics;
using REFLEKT.ONEAuthor.Application.Utilities;
using System.IO;
using System.Linq;

namespace REFLEKT.ONEAuthor.Application.Scenarios
{
    public class ScenarioService : IScenarioService
    {
        private readonly ILogger<ScenarioService> _logger;
        private readonly IJsonSerializerUtility _jsonSerializer;
        private readonly ITopicService _topicService;

        public ScenarioService(ILogger<ScenarioService> logger, IJsonSerializerUtility jsonSerializer, ITopicService topicService)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _topicService = topicService;
        }

        public string GetScenarioFolder(string scenarioId)
        {
            string result = ApiHelper.FindFolderById(scenarioId, PathHelper.GetRepoPath());
            if (!Directory.Exists(result))
            {
                _logger.LogWarning($"System cannot find directory of scenario with ID: {scenarioId}");

                throw new DirectoryNotFoundException($"System cannot find directory of scenario with ID: {scenarioId}");
            }

            return result;
        }

        public bool CheckIfScenarioIsBusy(string scenarioId)
        {
            var scenarioFolder = ApiHelper.FindFolderById(scenarioId, PathHelper.GetRepoPath());
            if (string.IsNullOrWhiteSpace(scenarioFolder))
            {
                return false;
            }

            return CheckForAnyEditingTopics(scenarioFolder) || CheckIfScenarioIsPublishing(scenarioFolder);
        }

        private bool CheckForAnyEditingTopics(string scenarioFolder)
        {
            var topicsFolder = Path.Combine(scenarioFolder, "topics");
            var editingTopics = _topicService.GetEditingTopicsFromFolder(topicsFolder);

            return editingTopics.Any();
        }

        private bool CheckIfScenarioIsPublishing(string scenarioFolder)
        {
            var infoFile = Path.Combine(scenarioFolder, new DirectoryInfo(scenarioFolder).Name + ".info");

            return _jsonSerializer.DeserializeFromFile<PublishedScenario>(infoFile).isPublishing;
        }
    }
}