using Newtonsoft.Json;

namespace REFLEKT.ONEAuthor.Application.Models
{
    public class UserModel
    {
        [JsonRequired]
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonRequired]
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("staticTicket", Required = Required.AllowNull)]
        public string StaticTicket { get; set; }

        [JsonProperty("draft")]
        public bool Draft { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }
    }
}