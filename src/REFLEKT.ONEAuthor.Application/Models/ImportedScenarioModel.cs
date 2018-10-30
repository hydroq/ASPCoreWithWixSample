using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using REFLEKT.ONEAuthor.Application.Models.Base;

namespace REFLEKT.ONEAuthor.Application.Models
{
    public class VersionModel
    {
        public string version { get; set; }
        public List<string> errors { get; set; }
    }

    public class XsltConfigModel
    {
        public string url { get; set; }
        public string ticket { get; set; }
        public int port { get; set; }
    }

    public class ScenariosModel
    {
        public List<Category> scenarios { get; set; }
        public List<string> errors { get; set; }
    }

    public class ScenarioModel
    {
        public ScenarioObject scenario { get; set; }
        public List<string> errors { get; set; }
    }

    public class ScenarioIdModel : IErrorResult
    {
        public string scenario_id { get; set; }

        public IEnumerable<string> errors { get; set; }
    }

    public class XmlUpdateModel
    {
        public string scenario_id { get; set; }
        public string topic_id { get; set; }
        public string message { get; set; }
        public List<string> errors { get; set; }
    }

    public class ScenarioIdWithFilepathModel
    {
        public string scenario_id { get; set; }
        public string file_path { get; set; }
        public List<string> errors { get; set; }
    }

    public class ScenarioStatusModel
    {
        public string scenario_id { get; set; }
        public string status { get; set; }
        public string path_to_info { get; set; }
        public bool isPub { get; set; }
        public List<string> errors { get; set; }
    }

    public class ScenarioPublicationStatusModel
    {
        public bool changes { get; set; }
        public bool draft { get; set; }
        public bool publication { get; set; }
        public List<string> errors { get; set; }
    }

    public class ImportedScenarioModel
    {
        public List<string[]> scenarios_ids { get; set; }
        public List<string> errors { get; set; }
    }

    public class ImportedTopicModel
    {
        public TopicObject topic { get; set; }
        public List<string> errors { get; set; }
    }

    public class ImportedTopicsModel
    {
        public List<TopicObject> topics { get; set; }
        public List<string> errors { get; set; }
    }

    public class LinkModel
    {
        public string link { get; set; }
        public List<string> errors { get; set; }
    }

    public class TopicReturnModel
    {
        public TopicObject topic { get; set; }
        public List<string> errors { get; set; }
    }

    public class TopicSortReturnModel
    {
        public List<SortTopicModel> topics { get; set; }
        public List<string> errors { get; set; }
    }

    public class TopicsModel
    {
        public List<TopicObject> topics { get; set; }
        public List<string> errors { get; set; }
    }

    public class TopicIdModel
    {
        public string topic_id { get; set; }
        public List<string> errors { get; set; }
    }

    public class TopicStatusModel
    {
        public string status { get; set; }
        public List<string> errors { get; set; }
    }

    public class FileLinkModel
    {
        public string file_url { get; set; }
        public List<string> errors { get; set; }
    }

    public class FolderModel : IErrorResult
    {
        public FolderObject folder { get; set; }

        public IEnumerable<string> errors { get; set; }
    }

    public class FolderIdModel
    {
        public string folder_id { get; set; }
        public List<string> errors { get; set; }
    }

    public class StatusModel : IErrorResult
    {
        public int status { get; set; }
        public IEnumerable<string> errors { get; set; }
    }

    public class CortonaPathModel : IErrorResult
    {
        public string path { get; set; }
        public IEnumerable<string> errors { get; set; }
    }

    public class HistoryVersionsModel
    {
        public List<int> versions { get; set; }
        public int current_version { get; set; }
        public List<string> errors { get; set; }
    }

    public class HistoryVersionModel
    {
        public string current { get; set; }
        public string requested { get; set; }
        public string scenario_id { get; set; }
        public List<string> errors { get; set; }
    }

    public class HistoryTopicVersion
    {
        public string topic_id { get; set; }
        public string topic_type { get; set; }
        public string topic_name { get; set; }
        public string current { get; set; }
        public string requested { get; set; }
        public List<string> errors { get; set; }
    }

    public class HistorySwitchVersion
    {
        public int current_version { get; set; }
        public string message { get; set; }
        public List<string> errors { get; set; }
    }

    public class HistoryScenarioVersion
    {
        public int current_version { get; set; }
        public string message { get; set; }
        public DescModel scenario { get; set; }
        public List<TopicObject> topics { get; set; }
        public List<string> errors { get; set; }
    }

    public class CommitModel
    {
        public List<string> errors { get; set; }
    }

    public class CommitStatusModel
    {
        public string status { get; set; }
        public List<string> errors { get; set; }
    }
}