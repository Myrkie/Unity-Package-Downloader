using System.Net;
using System.Text.Json;
using Serilog;
using Unity_package_downloader.Json.ProductInfo;
using Unity_package_downloader.Json.Products;
using Unity_package_downloader.Json.Purchases;

namespace Unity_package_downloader
{
    public class WebRequests : HttpClientAuth
    {
        private readonly ILogger _logger = Log.ForContext<WebRequests>();
        private readonly List<ResponseStruct> _responses = [];
        private readonly List<string> _productIDs = [];

        private struct ResponseStruct
        {
            public string Id;
            public string Name;
            public string DownloadUrl;
            public byte[]? AesKey;
            public string? Version;
            public string Author;
            public string Image;
        }

        private bool _endReached;

        public async Task GetProductIds(string token, bool limited = false)
        {
            SetBearerToken(token);

            for (var offset = 0; !_endReached; offset += 15)
            {
                await GetPurchases(offset);

                if (!limited) continue;
                _logger.Information("Limited mode set, downloading only first page");
                break;
            }

            await GetProductInfo();
        }


        private async Task GetPurchases(int offset)
        {
            _logger.Information("Getting page {PurchaseOffset}", offset);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            using var response =
                await client.GetAsync($"https://packages-v2.unity.com/-/api/purchases?offset={offset}&limit=15&query=");
            response.EnsureSuccessStatusCode();
            var responseBodyPurchases = await response.Content.ReadAsStringAsync();

            var deserializePurchasesJson = JsonSerializer.Deserialize(responseBodyPurchases, PurchaseJsonContext.Default.PurchaseRoot);
            
            if (deserializePurchasesJson?.results is { Length: > 0 })
            {
                _endReached = false;
                foreach (var result in deserializePurchasesJson.results)
                {
                    _productIDs.Add(result.packageId.ToString());
                }
            }
            else _endReached = true;
        }

        private async Task GetProductInfo()
        {
            var tasks = _productIDs.Select(async responsePackage =>
            {
                var urlInfo = $"https://packages-v2.unity.com/-/api/legacy-package-download-info/{responsePackage}";
                var response = await client.GetAsync(urlInfo);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Information("Package not downloadable {ResponsePackage}", responsePackage);
                    return;
                }

                var urlProduct = $"https://packages-v2.unity.com/-/api/product/{responsePackage}";
                var responseProduct = await client.GetAsync(urlProduct);
                var responseBodyProduct = await responseProduct.Content.ReadAsStringAsync();
                var deserializeProductJson = JsonSerializer.Deserialize(responseBodyProduct, ProductJsonContext.Default.ProductRoot);

                _logger.Information("Downloading Json of: {ResponsePackage}", responsePackage);

                var responseBodyInfo = await response.Content.ReadAsStringAsync();
                var deserializeProductinfo = JsonSerializer.Deserialize(responseBodyInfo, ProductInfoJsonContext.Default.ProductInfoRoot);

                if (deserializeProductinfo?.result.download != null)
                {
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                    var imageUrl = deserializeProductJson?.mainImage.big ?? deserializeProductJson?.mainImage.big_v2;
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        var responseStruct = new ResponseStruct
                        {
                            DownloadUrl = deserializeProductinfo.result.download.url,
                            Id = deserializeProductinfo.result.download.id,
                            Author = deserializeProductinfo.result.download.filename_safe_publisher_name,
                            AesKey = !string.IsNullOrEmpty(deserializeProductinfo.result.download.key)
                                ? Convert.FromHexString(deserializeProductinfo.result.download.key)
                                : [],
                            Name = deserializeProductinfo.result.download.filename_safe_package_name,
                            Image = $"https://{imageUrl.Replace(@"\", "/").Remove(0, 2)}",
                            Version = deserializeProductJson?.version.name
                        };

                        lock (_responses)
                        {
                            _responses.Add(responseStruct);
                        }
                    }
                }
                else
                {
                    _logger.Error("Failed to deserialize information for package {ErrorPackage}", responsePackage);
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task DownloadProducts(string path)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var downloads in _responses)
            {
                _logger.Information("Asset name: {AssetName} | Asset ID: {AssetID}", downloads.Name, downloads.Id);
                var trimmedName = downloads.Name.Replace("-", "").Replace(".", "").Replace(" ", ".").Replace("..", ".");
                var formattedName = $"{downloads.Author.Replace(" ", ".")}_UnityAsset_{trimmedName}(V{downloads.Version})_{downloads.Id}";

                DirectoryInfo info = new DirectoryInfo(path);
                if (!info.Exists)
                {
                    info.Create();
                }

                if (File.Exists($"{path}\\{formattedName}.jpg"))
                {
                    _logger.Information("File exists aborting: {FileDownload}.jpg", downloads.Name);
                    continue;
                }

                _logger.Information("Downloading Image: {Image}", downloads.Image);
                await DownloadImage(downloads.Image, $"{path}\\{formattedName}.jpg");

                var hasAesKey = downloads.AesKey is { Length: > 0 };
                var outputPath = $"{path}\\{formattedName}.unitypackage";
                var encryptedPath = $"{path}\\{formattedName}_Encrypted.AES";

                if (File.Exists(outputPath))
                {
                    _logger.Information("File exists aborting: {FileDownload}", downloads.Name);
                    continue;
                }

                _logger.Information("Downloading File: {FileDownload}", downloads.DownloadUrl);

                if (hasAesKey)
                {
                    await DownloadFile(downloads.DownloadUrl, encryptedPath);

                    _logger.Information("Starting Decryption");

                    await Decryption.Decryptor.DecryptFile(encryptedPath, outputPath, downloads.AesKey[..32], downloads.AesKey[32..]);

                    _logger.Information("Decryption Finished");

                    File.Delete(encryptedPath);
                }
                else
                {
                    await DownloadFile(downloads.DownloadUrl, outputPath);
                }
            }
        }
    }
}