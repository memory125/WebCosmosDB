namespace todo.Models
{
    using Microsoft.Azure.Documents;
    using Newtonsoft.Json;

    public class UserItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty(PropertyName = "created by")]
        public string CreatedBy { get; set; }
    }
}