using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TaskPrint.Models.Wildberries
{

    class Order
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("rid")]
        public string Rid { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("warehouseId")]
        public int WarehouseId { get; set; }

        [JsonProperty("supplyId")]
        public string SupplyId { get; set; }

        [JsonProperty("offices")]
        public List<string> Offices { get; set; }

        [JsonProperty("address")]
        public Address Address { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("skus")]
        public List<string> Skus { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("convertedPrice")]
        public int ConvertedPrice { get; set; }

        [JsonProperty("currencyCode")]
        public int CurrencyCode { get; set; }

        [JsonProperty("convertedCurrencyCode")]
        public int ConvertedCurrencyCode { get; set; }

        [JsonProperty("orderUid")]
        public string OrderUid { get; set; }

        [JsonProperty("deliveryType")]
        public string DeliveryType { get; set; }

        [JsonProperty("nmId")]
        public int NmId { get; set; }

        [JsonProperty("chrtId")]
        public int ChrtId { get; set; }

        [JsonProperty("article")]
        public string Article { get; set; }

        [JsonProperty("isLargeCargo")]
        public bool IsLargeCargo { get; set; }

        public OrderStikers Stickers { get; set; }
    }
}
