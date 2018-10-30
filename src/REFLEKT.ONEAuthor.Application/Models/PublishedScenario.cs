using System.Collections.Generic;
using Newtonsoft.Json;

namespace REFLEKT.ONEAuthor.Application.Models
{
    public class PublishedScenario
    {
        public string title { get; set; }
        public string desc { get; set; }
        public int id { get; set; }
        public bool isPublishing { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public string id { get; set; }
        public long modified_utc { get; set; }
        public List<Product> products { get; set; }
    }

    public class Product
    {
        public string name { get; set; }
        public string id { get; set; }
        public long modified_utc { get; set; }
        public List<Scenario> scenarios { get; set; }
    }

    public class Scenario
    {
        public string id { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
        public string draft { get; set; }
        public string description { get; set; }
        public long modified_utc { get; set; }
    }

    public class TopicModel
    {
        public string type { get; set; }
        public string name { get; set; }
        public int index { get; set; }
        public string unique_id { get; set; }

        [JsonIgnore]
        public string pathToZip
        {
            get
            {
                if (localization == "ru_ru")
                    return pathToZipRU;
                else if (localization == "en_en")
                    return pathToZipEN;
                else
                    return pathToZipDEFAULT;
            }
            set
            {
                if (localization == "ru_ru")
                    pathToZipRU = value;
                else if (localization == "en_en")
                    pathToZipEN = value;
                else
                    pathToZipDEFAULT = value;
            }
        }

        [JsonProperty] public string pathToZipDEFAULT { get; set; }
        [JsonProperty] private string pathToZipRU { get; set; }
        [JsonProperty] private string pathToZipEN { get; set; }
        public int isOpened { get; set; }

        [JsonIgnore]
        public string vmpPath
        {
            get
            {
                if (localization == "ru_ru")
                    return vmpPathRU;
                else if (localization == "en_en")
                    return vmpPathEN;
                else
                    return vmpPathDEFAULT;
            }
            set
            {
                if (localization == "ru_ru")
                    vmpPathRU = value;
                else if (localization == "en_en")
                    vmpPathEN = value;
                else
                    vmpPathDEFAULT = value;
            }
        }

        [JsonProperty] public string vmpPathDEFAULT { get; set; }
        [JsonProperty] private string vmpPathRU { get; set; }
        [JsonProperty] private string vmpPathEN { get; set; }
        [JsonIgnore] public string infoPath { get; set; }

        //[JsonIgnore]
        public string localization { get; set; }
    }

    public class OldTopicModel
    {
        public string type { get; set; }
        public string name { get; set; }
        public int index { get; set; }
        public string pathToZip { get; set; }
        public int isOpened { get; set; }
        public string vmpPath { get; set; }
    }

    public class CategoryModel
    {
        public string title { get; set; }
        public List<CategoryModel> childs { get; set; }
    }

    public class DescModel
    {
        public string title { get; set; }
        public string desc { get; set; }
    }
}