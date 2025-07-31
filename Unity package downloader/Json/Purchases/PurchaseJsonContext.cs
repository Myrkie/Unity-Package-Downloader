using System.Text.Json.Serialization;

namespace Unity_package_downloader.Json.Purchases
{
    [JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(PurchaseRoot))]
    public partial class PurchaseJsonContext : JsonSerializerContext;
}