using Newtonsoft.Json;

namespace TaskPrint.Models.Wildberries
{
    class OrderSatatus
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("supplierStatus")]
        public string SupplierStatus { get; set; }

        [JsonProperty("wbStatus")]
        public string WbStatus { get; set; }
    }
}
