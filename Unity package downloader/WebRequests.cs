using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Serilog;
using Unity_package_downloader.Json;

namespace Unity_package_downloader;

public class WebRequests
{
    private static string token = "LHd_djOrSSn2XBO0hqo-mVd-Oj4nr8ZZybX6GO9T57c002f";
    private static readonly ILogger Logger = Log.ForContext(typeof(WebRequests));
    private static readonly List<ResponseStruct> Responses = [];
    private static readonly List<string> ProductIDs = [];
    private static readonly MediaTypeWithQualityHeaderValue Accept = new("*/*");
    private static readonly StringWithQualityHeaderValue Deflate = new("deflate");
    private static readonly StringWithQualityHeaderValue Gzip = new("gzip");
    
    private static HttpClientHandler handler = new()
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    };
    private static readonly HttpClient Client = new(handler)
    {
        DefaultRequestHeaders =
        {
            Accept = { Accept },
            AcceptEncoding = { Deflate, Gzip },
            Authorization = new AuthenticationHeaderValue("Bearer", token)
        }
    };

    struct ResponseStruct
    {
        public string? Id;
        public string? Name;
        public string? DownloadURL;
        public byte[]? AESKey;
        public string? Version;
        public string? Author;
        public string? Image;
    }

    private static bool endreached;

    public static async Task GetProductIds()
    {
        for (var offset= 0; !endreached;  offset += 15)
        {
            await GetPurchases (offset);
        }
    }
    

    private static async Task GetPurchases(int offset)
    {
        Logger.Information("getting page {PurchaseOffset}", offset);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        using var response =
            await Client.GetAsync($"https://packages-v2.unity.com/-/api/purchases?offset={offset}&limit=15&query=");
        response.EnsureSuccessStatusCode();
        var responsebody = await response.Content.ReadAsStringAsync();
        

        var deserializePurchasesJson = JsonSerializer.Deserialize<PurchasesJSON.RootObject>(responsebody);

        if (deserializePurchasesJson.results != null && deserializePurchasesJson.results.Length > 0)
        {
            endreached = false;
            foreach (var result in deserializePurchasesJson.results)
            {
                ProductIDs.Add(result.packageId.ToString());
            }
        }
        else endreached = true;
    }
    
    public static async Task GetProductInfo()
    {
        var tasks = ProductIDs.Select(async responsePackage =>
        {
            var urlInfo = $"https://packages-v2.unity.com/-/api/legacy-package-download-info/{responsePackage}";
            var responseinfo = await Client.GetAsync(urlInfo);
            if (responseinfo.StatusCode != HttpStatusCode.OK)
            {
                Logger.Information("Package not downloadable {responsePackage}", responsePackage);
                return;
            }

            var urlProduct = $"https://packages-v2.unity.com/-/api/product/{responsePackage}";
            var responseProduct = await Client.GetAsync(urlProduct);
            var responsebodyProduct = await responseProduct.Content.ReadAsStringAsync();
            var deserializeProductJson = JsonSerializer.Deserialize<ProductJSON.RootObject>(responsebodyProduct);

            Logger.Information("Downloading Json of: {responsePackage}", responsePackage);

            var responsebodyInfo = await responseinfo.Content.ReadAsStringAsync();
            var deserializeProductinfo = JsonSerializer.Deserialize<ProductInfoJSON.RootObject>(responsebodyInfo);

            if (deserializeProductinfo?.result.download != null)
            {
                var imageUrl = deserializeProductJson.mainImage.big ?? deserializeProductJson.mainImage.big_v2;
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var sResponsestruct = new ResponseStruct
                    {
                        DownloadURL = deserializeProductinfo.result.download.url,
                        Id = deserializeProductinfo.result.download.id,
                        Author = deserializeProductinfo.result.download.filename_safe_publisher_name,
                        AESKey = !string.IsNullOrEmpty(deserializeProductinfo.result.download.key)
                            ? Convert.FromHexString(deserializeProductinfo.result.download.key)
                            : Array.Empty<byte>(),
                        Name = deserializeProductinfo.result.download.filename_safe_package_name,
                        Image = $"http://{imageUrl.Replace(@"\", "/").Remove(0, 2)}",
                        Version = deserializeProductJson.version?.name
                    };

                    lock (Responses)
                    {
                        Responses.Add(sResponsestruct);
                    }
                }
            }
            else
            {
                Logger.Error("Failed to deserialize information for package {errorPackage}", responsePackage);
            }
        });

        await Task.WhenAll(tasks);
    }

    public static async Task DownloadProducts(string path)
    {
        foreach (var downloads in Responses)
        {
            Logger.Information("asset name: {assetName}", downloads.Name);
            var trimmedname = downloads.Name.Replace("-", "").Replace(".", "").Replace(" ", ".").Replace("..", ".");
            Logger.Information("asset name trimmed: {trimmedAssetName}", trimmedname);
            var name = string.Concat($"{downloads.Author.Replace(" ", ".")}_UnityAsset_{trimmedname}(V{downloads.Version})_{downloads.Id}");

            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                info.Create();
            }

            if (File.Exists($"{path}\\{name}.jpg"))
            {
                Logger.Information("file exists aborting: {fileDownload}.jpg", downloads.Name);
                continue;
            }
            var i = new WebClient();
            Logger.Information("Downloading Image: {image}", downloads.Image);
            await i.DownloadFileTaskAsync(downloads.Image, $"{path}\\{name}.jpg");
            
            
            var c = new MyrkieWebClient();
            c.Timeout = 60 * 60 * 60 * 60;
            if (File.Exists($"{path}\\{name}.unitypackage"))
            {
                Logger.Information("file exists aborting: {fileDownload}", downloads.Name);
                continue;
            }
            
            Logger.Information("downloading File: {fileDownload}", downloads.DownloadURL);
            await c.DownloadFileTaskAsync(downloads.DownloadURL, $"{path}\\{name}_Encrypted.AES");

            if (downloads.AESKey.Length > 0)
            {
                Logger.Information("Starting Decryption");
                await Decryption.Decryption.DecryptString($"{path}\\{name}_Encrypted.AES", $"{path}\\{name}.unitypackage", downloads.AESKey[..32], downloads.AESKey[32..]);
                Logger.Information("Decryption Finished");
                File.Delete($"{path}\\{name}_Encrypted.AES");
            }else File.Move($"{path}\\{name}_Encrypted.AES",$"{path}\\{name}.unitypackage");

        }
    }
}

public class MyrkieWebClient : WebClient
{
    /// <summary>
    /// Default constructor (30000 ms timeout)
    /// NOTE: timeout can be changed later on using the [Timeout] property.
    /// </summary>
    public MyrkieWebClient() : this(30000) { }
 
    /// <summary>
    /// Constructor with customizable timeout
    /// </summary>
    /// <param name="timeout">
    /// Web request timeout (in milliseconds)
    /// </param>
    public MyrkieWebClient(int timeout)
    {
        Timeout = timeout;
    }
 
    #region Methods
    protected override WebRequest GetWebRequest(Uri uri)
    {
        WebRequest w = base.GetWebRequest(uri);
        w.Timeout = Timeout;
        ((HttpWebRequest)w).ReadWriteTimeout = Timeout;
        return w;
    }
    #endregion
 
    /// <summary>
    /// Web request timeout (in milliseconds)
    /// </summary>
    public int Timeout { get; set; }
}