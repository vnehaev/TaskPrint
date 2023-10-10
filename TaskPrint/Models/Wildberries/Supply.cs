using Newtonsoft.Json;
using System;

namespace TaskPrint.Models.Wildberries
{
    class Supply
    {
        [JsonProperty("closedAt")]
        public DateTime? ClosedAt { get; set; }

        [JsonProperty("scanDt")]
        public DateTime? ScanDt { get; set; }

        [JsonProperty("isLargeCargo")]
        public bool IsLargeCargo { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("done")]
        public bool Done { get; set; }
    }
}
