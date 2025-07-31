using System.Text.Json.Serialization;

namespace Unity_package_downloader.Json.Products
{
    [JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ProductRoot))]
    public partial class ProductJsonContext : JsonSerializerContext;
}