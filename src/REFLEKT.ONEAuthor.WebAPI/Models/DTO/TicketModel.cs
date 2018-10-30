using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace REFLEKT.ONEAuthor.WebAPI.Models.DTO
{
    public class TicketModel
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
        
        [DefaultValue(typeof(List<string>), "")]
        [JsonProperty("errors", DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Errors { get; set; }
    }
}