using Newtonsoft.Json;

namespace TaskPrint.Models.Wildberries
{
    internal class OrderStikers
    {
        [JsonProperty("partA")]
        public string PartA { get; set; }

        [JsonProperty("partB")]
        public string PartB { get; set; }

        [JsonProperty("barcode")]
        public string Barcode { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }
    }
}
