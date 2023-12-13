using System.Collections.Generic;
using Newtonsoft.Json;


public class Characteristic
{
    [JsonProperty("Вес товара с упаковкой (г)")]
    public float WeightNetto  { get; set; }

    [JsonProperty("Модель")]
    public List<string> Model { get; set; }

    [JsonProperty("Гарантийный срок")]
    public List<string> WarrantyPeriod { get; set; }

    [JsonProperty("Упаковка")]
    public List<string> PackagingType { get; set; }

    [JsonProperty("Комплектация")]
    public List<string> Equipment { get; set; }

    [JsonProperty("Материал корпуса")]
    public List<string> BodyMaterial { get; set; }

    [JsonProperty("Питание")]
    public List<string> PowerSupply { get; set; }

    [JsonProperty("Противопоказания")]
    public List<string> Contraindications { get; set; }

    [JsonProperty("Предмет")]
    public string Subject { get; set; }

    [JsonProperty("Наименование")]
    public string ProductName { get; set; }
}

public class Size
{
    [JsonProperty("techSize")]
    public string TechnicalSize { get; set; }

    [JsonProperty("skus")]
    public List<string> Skus { get; set; }

    [JsonProperty("wbSize")]
    public string WbSize { get; set; }

    [JsonProperty("price")]
    public double Price { get; set; }
}

public class Data
{
    [JsonProperty("imtID")]
    public int ImtID { get; set; }

    [JsonProperty("object")]
    public string Object { get; set; }

    [JsonProperty("objectID")]
    public int ObjectID { get; set; }

    [JsonProperty("nmID")]
    public int NmID { get; set; }

    [JsonProperty("vendorCode")]
    public string VendorCode { get; set; }

    [JsonProperty("mediaFiles")]
    public List<string> MediaFiles { get; set; }

    [JsonProperty("sizes")]
    public List<Size> Sizes { get; set; }

    [JsonProperty("characteristics")]
    public List<Characteristic> Characteristics { get; set; }

    [JsonProperty("isProhibited")]
    public bool IsProhibited { get; set; }

    [JsonProperty("tags")]
    public List<Tag> Tags { get; set; }
}

public class Tag
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }
}

public class ApiResponseProducts
{
    [JsonProperty("data")]
    public List<Data> Data { get; set; }

    [JsonProperty("error")]
    public bool Error { get; set; }

    [JsonProperty("errorText")]
    public string ErrorText { get; set; }

    [JsonProperty("additionalErrors")]
    public object AdditionalErrors { get; set; }
}

