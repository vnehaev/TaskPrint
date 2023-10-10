using System.Collections.Generic;
using Newtonsoft.Json;

public class Characteristics
{
    [JsonProperty("Вес товара с упаковкой (г)")]
    public double WeightWithPackaging { get; set; }

    [JsonProperty("Вес товара без упаковки (г)")]
    public double WeightWithoutPackaging { get; set; }

    [JsonProperty("Высота предмета")]
    public double Height { get; set; }

    [JsonProperty("Ширина предмета")]
    public double Width { get; set; }

    [JsonProperty("Ширина упаковки")]
    public double PackagingWidth { get; set; }

    [JsonProperty("Высота упаковки")]
    public double PackagingHeight { get; set; }

    [JsonProperty("Длина упаковки")]
    public double PackagingLength { get; set; }

    [JsonProperty("Количество предметов в упаковке")]
    public List<string> ItemsInPackaging { get; set; }

    [JsonProperty("Бренд")]
    public string Brand { get; set; }

    [JsonProperty("Страна производства")]
    public List<string> CountryOfOrigin { get; set; }

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
    public string Name { get; set; }

    [JsonProperty("Описание")]
    public string Description { get; set; }
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
    public List<Characteristics> Characteristics { get; set; }

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
