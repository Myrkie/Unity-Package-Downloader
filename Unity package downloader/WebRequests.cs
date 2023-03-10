using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Unity_package_downloader.Decryption;
using Unity_package_downloader.Json;

namespace Unity_package_downloader;

public class WebRequests
{
    
    private static string token = "MKgiDNVrYSMjcJRVV07LciTdqkTgoAv5nmaosAg-tAM002f";
    private static readonly List<responsestruct> Responses = new();
    private static readonly List<string> ids = new();
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
    
    
    private static readonly HttpClient Client2 = new(handler)
    {
        DefaultRequestHeaders =
        {
            Accept = { Accept },
            AcceptEncoding = { Deflate, Gzip }
            
        }
    };
    
    

    struct responsestruct
    {
        public string? Id;
        public string? Name;
        public string? DownloadURL;
        public byte[]? AESKey;
        public string? Version;
        public string? Author;
        public responsestruct(string? id, string? name, string? downloadUrl, byte[]? aesKey, string? version)
        {
            Id = id;
            Name = name;
            DownloadURL = downloadUrl;
            AESKey = aesKey;
            Version = version;
        }
    }

    private static bool endreached;

    public static async Task loop()
    {
        for (var offset= 0; !endreached;  offset += 15)
        {
            await GetPurchases (offset);
        }

        await GetInfo();
    }
    

    private static async Task GetPurchases(int offset)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        using var response =
            await Client.GetAsync($"https://packages-v2.unity.com/-/api/purchases?offset={offset}&limit=15&query=");
        response.EnsureSuccessStatusCode();
        var responsebody = await response.Content.ReadAsStringAsync();
        

        var deserializePurchasesJson = JsonSerializer.Deserialize<PurchasesJSON.RootObject>(responsebody);

        if (deserializePurchasesJson.results != null)
        {
            endreached = false;
            foreach (var result in deserializePurchasesJson.results)
            {
                ids.Add(result.packageId.ToString());
            }
        }
        else endreached = true;
    }
    
    private static async Task GetInfo()
    {
        var sResponsestruct = new responsestruct();
        foreach (var responsePackage in ids)
        {
            Thread.Sleep(500);
            var responseProduct = await Client.GetAsync($"https://packages-v2.unity.com/-/api/product/{responsePackage}");
            responseProduct.EnsureSuccessStatusCode();
            
            var responsebodyProduct = await responseProduct.Content.ReadAsStringAsync();
            var deserializeProductJson = JsonSerializer.Deserialize<ProductJSON.RootObject>(responsebodyProduct);
            
            using var responseinfo = await Client.GetAsync($"https://packages-v2.unity.com/-/api/legacy-package-download-info/{responsePackage}");

            if (responseinfo.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Package not downloadable {responsePackage}");
                continue;
            }
            Console.WriteLine($"downloading Json: {responsePackage}");

            var responsebodyInfo = await responseinfo.Content.ReadAsStringAsync();

            var deserializeProductinfo = JsonSerializer.Deserialize<ProductInfoJSON.RootObject>(responsebodyInfo);


            if (deserializeProductinfo != null)
            {
                sResponsestruct.DownloadURL = deserializeProductinfo.result.download.url;
                sResponsestruct.Id = deserializeProductinfo.result.download.id;
                sResponsestruct.Author = deserializeProductinfo.result.download.filename_safe_publisher_name;
                var bytes = Convert.FromHexString(deserializeProductinfo.result.download.key);
                sResponsestruct.AESKey = bytes;
                sResponsestruct.Name = deserializeProductinfo.result.download.filename_safe_package_name;
            }

            if (deserializeProductJson != null) sResponsestruct.Version = deserializeProductJson.version.name;
            
            Responses.Add(sResponsestruct);
            
            
        }

        await DownloadShit();
    }

    private static async Task DownloadShit()
    {
        foreach (var downloads in Responses)
        {
            var name = string.Concat($"assetstore_Tool_{downloads.Name}({downloads.Version})_{downloads.Id}");
            var path = $"G:\\WINDownloads\\upload\\test\\{downloads.Author}";
            
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                info.Create();
            }
            
            var c = new MyrkieWebClient();
            c.Timeout = 60 * 60 * 60 * 60;
            if (File.Exists($"{path}\\{name}.unitypackage"))
            {
                Console.WriteLine($"file exists aborting: {downloads.Name}");
                continue;
            }
            Console.WriteLine($"downloading: {downloads.Name}");

            await c.DownloadFileTaskAsync(downloads.DownloadURL, $"{path}\\{name}_Encrypted.AES");
            //using var downloadresp = await Client2.GetStreamAsync(downloads.DownloadURL);
            // if (downloadresp.StatusCode != HttpStatusCode.OK)
            // {
            //     Console.WriteLine(downloadresp.Headers.WwwAuthenticate);
            //     Console.WriteLine($"Package not downloadable {downloads.Id}");
            //     continue;
            // }
            
            
            //var responsebodyInfo = await downloadresp.Content.ReadAsStreamAsync();

            //SaveStreamAsFile($"{path}", downloadresp, $"{name}_Encrypted");
            
            
            
            if (downloads.AESKey.Length > 0)
            {
                Decrpytion.DecryptString($"{path}\\{name}_Encrypted.AES", $"{path}\\{name}.unitypackage", downloads.AESKey[..32], downloads.AESKey[32..]);
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
 
    public new async Task<string> DownloadStringTaskAsync(Uri address)
    {
        var t = base.DownloadStringTaskAsync(address);
        if (await Task.WhenAny(t, Task.Delay(Timeout)) != t)
            CancelAsync();
        return await t;
    }
    #endregion
 
    /// <summary>
    /// Web request timeout (in milliseconds)
    /// </summary>
    public int Timeout { get; set; }
}