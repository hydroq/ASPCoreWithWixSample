using System.Collections.Generic;

namespace REFLEKT.ONEAuthor.Application.Models
{
    public class ScenarioObject
    {
        public string id;
        public string name;
        public string description;
        public long modified_utc;
        public List<TopicObject> topics;
    }

    public class ScenarioPatchObject
    {
        public string id;
        public string name;
        public string description;
        public List<string> topics;
    }

    public class FolderObject
    {
        public string id;
        public string type;
        public string name;
        public string parent;
        public long modified_utc;
    }

    public class TopicObject
    {
        public string id;
        public string type;
        public string name;
        public string tracking_config_url;
        public string unique_id;
    }

    public enum FolderType
    {
        Category,
        Product
    }

    public class Base64File
    {
        public string name;
        public string file;
        public string type;
    }

    public class ApiFormInput
    {
        public string ticket { get; set; }

        public string username { get; set; }
        public string password { get; set; }
        public string server { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public string file { get; set; }
        public string xml { get; set; }
        public string tracking_configuration { get; set; } = "";
        public bool overRide { get; set; } = false;
        public bool local { get; set; } = false;
        public string scenario { get; set; }
        public FolderNameModel folder { get; set; }
        public string folder_id { get; set; }
        public string key { get; set; }
        public string type { get; set; }
        public string filename { get; set; }
        public int index { get; set; }
        public string path { get; set; }
        public bool unique_id { get; set; } = false;
        public string user { get; set; }

        public List<SortTopicModel> topics { get; set; }
    }

    public class SortTopicModel
    {
        public int old_id { get; set; }
        public int new_id { get; set; }
        public string path { get; set; }
    }

    public class ApiError
    {
        public List<string> errors { get; set; }
        public int status { get; set; }
    }

    public class FolderNameModel
    {
        public string name { get; set; }
        public string description { get; set; }
    }
    
    public class XSPConfigModel
    {
        public int port { get; set; }
        public List<string> errors { get; set; }
    }
}