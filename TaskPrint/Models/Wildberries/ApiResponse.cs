using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing;

namespace TaskPrint.Models.Wildberries
{
    class ApiResponseOrders
    {
        [JsonProperty("next")]
        public long Next { get; set; }

        [JsonProperty("orders")]
        public List<Order> Orders { get; set; }

    }

    class ApiResponseSupplies
    {
        [JsonProperty("next")]
        public long Next { get; set; }

        [JsonProperty("supplies")]
        public List<Supply> Supplies { get; set; }
    }

    class ApiResponseStickers
    {
        [JsonProperty("stickers")]
        public List<OrderStikers> Stickers { get; set; }
    }
}
