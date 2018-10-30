using Newtonsoft.Json;

namespace REFLEKT.ONEAuthor.Application.Settings
{
    public class CortonaConfigModel
    {
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}