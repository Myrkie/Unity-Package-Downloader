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
            public string? Id;
            public string? Name;
            public string? DownloadUrl;
            public byte[]? AesKey;
            public string? Version;
            public string? Author;
            public string? Image;
        }

        private bool _endReached;

        public async Task GetProductIds(string token)
        {
            SetBearerToken(token);
            for (var offset = 0; !_endReached; offset += 15)
            {
                await GetPurchases(offset);
            }

            await GetProductInfo();
        }


        private async Task GetPurchases(int offset)
        {
            _logger.Information("getting page {PurchaseOffset}", offset);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            using var response =
                await client.GetAsync($"https://packages-v2.unity.com/-/api/purchases?offset={offset}&limit=15&query=");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();


            var deserializePurchasesJson = JsonSerializer.Deserialize<PurchaseRoot>(content);

            if (deserializePurchasesJson.results is { Length: > 0 })
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
                var responseinfo = await client.GetAsync(urlInfo);
                if (responseinfo.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Information("Package not downloadable {responsePackage}", responsePackage);
                    return;
                }

                var urlProduct = $"https://packages-v2.unity.com/-/api/product/{responsePackage}";
                var responseProduct = await client.GetAsync(urlProduct);
                var responsebodyProduct = await responseProduct.Content.ReadAsStringAsync();
                var deserializeProductJson = JsonSerializer.Deserialize<ProductRoot>(responsebodyProduct);

                _logger.Information("Downloading Json of: {responsePackage}", responsePackage);

                var responsebodyInfo = await responseinfo.Content.ReadAsStringAsync();
                var deserializeProductinfo = JsonSerializer.Deserialize<ProductInfoRoot>(responsebodyInfo);

                if (deserializeProductinfo?.result.download != null)
                {
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                    var imageUrl = deserializeProductJson.mainImage.big ?? deserializeProductJson.mainImage.big_v2;
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        var sResponsestruct = new ResponseStruct
                        {
                            DownloadUrl = deserializeProductinfo.result.download.url,
                            Id = deserializeProductinfo.result.download.id,
                            Author = deserializeProductinfo.result.download.filename_safe_publisher_name,
                            AesKey = !string.IsNullOrEmpty(deserializeProductinfo.result.download.key)
                                ? Convert.FromHexString(deserializeProductinfo.result.download.key)
                                : [],
                            Name = deserializeProductinfo.result.download.filename_safe_package_name,
                            Image = $"http://{imageUrl.Replace(@"\", "/").Remove(0, 2)}",
                            Version = deserializeProductJson.version.name
                        };

                        lock (_responses)
                        {
                            _responses.Add(sResponsestruct);
                        }
                    }
                }
                else
                {
                    _logger.Error("Failed to deserialize information for package {errorPackage}", responsePackage);
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task DownloadProducts(string path)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var downloads in _responses)
            {
                _logger.Information("Asset name: {assetName} | Asset ID: {assetID}", downloads.Name, downloads.Id);
                var trimmedName = downloads.Name.Replace("-", "").Replace(".", "").Replace(" ", ".").Replace("..", ".");
                var formattedName = string.Concat($"{downloads.Author.Replace(" ", ".")}_UnityAsset_{trimmedName}(V{downloads.Version})_{downloads.Id}");

                DirectoryInfo info = new DirectoryInfo(path);
                if (!info.Exists)
                {
                    info.Create();
                }

                if (File.Exists($"{path}\\{formattedName}.jpg"))
                {
                    _logger.Information("File exists aborting: {fileDownload}.jpg", downloads.Name);
                    continue;
                }

                _logger.Information("Downloading Image: {image}", downloads.Image);
                await DownloadImage(downloads.Image, $"{path}\\{formattedName}.jpg");

                if (File.Exists($"{path}\\{formattedName}.unitypackage"))
                {
                    _logger.Information("File exists aborting: {fileDownload}", downloads.Name);
                    continue;
                }

                _logger.Information("Downloading File: {fileDownload}", downloads.DownloadUrl);
                await DownloadFile(downloads.DownloadUrl, $"{path}\\{formattedName}_Encrypted.AES");

                if (downloads.AesKey.Length > 0)
                {
                    _logger.Information("Starting Decryption");
                    await Decryption.Decryption.DecryptString($"{path}\\{formattedName}_Encrypted.AES",
                        $"{path}\\{formattedName}.unitypackage", downloads.AesKey[..32], downloads.AesKey[32..]);
                    _logger.Information("Decryption Finished");
                    File.Delete($"{path}\\{formattedName}_Encrypted.AES");
                }
                else File.Move($"{path}\\{formattedName}_Encrypted.AES", $"{path}\\{formattedName}.unitypackage");
            }
        }
    }
}