using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialShared.Converters;

namespace SocialFacesApp.Models
{
    [Serializable]
    public class FacesAnalysisResult
    {
        [JsonConverter(typeof(PostedOnConverter))]
        [JsonProperty("postedOn")]
        public DateTime PostedOn { get; set; }

        [JsonProperty("descriptors")]
        public JArray Descriptors { get; set; }
    }
}