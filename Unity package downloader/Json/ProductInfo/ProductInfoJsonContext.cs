using System.Text.Json.Serialization;

namespace Unity_package_downloader.Json.ProductInfo
{
    [JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ProductInfoRoot))]
    public partial class ProductInfoJsonContext : JsonSerializerContext;
}