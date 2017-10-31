using Newtonsoft.Json;
using System.Collections.Generic;

namespace AdsDashboard.Models
{
    public class AdsItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "duration")]
        public int Duration { get; set; }

        [JsonProperty(PropertyName = "first impression")]
        public int FirstImpression { get; set; }

        [JsonProperty(PropertyName = "impression interval")]
        public int ImpressionInterval { get; set; }

    }
}