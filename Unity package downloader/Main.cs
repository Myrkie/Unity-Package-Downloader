using Unity_package_downloader;

class aes
{
    // todo: Oath shit
    static async Task Main(string[] args)
    {
        
        var path = "";
        if (args.Length == 0)
        {
            path = @"G:\WINDownloads\upload";
        }
        else path = args[0];
        
        await WebRequests.GetProductIds();
        
        await WebRequests.GetProductInfo();

        await WebRequests.DownloadProducts(path);
        Thread.Sleep(5000);
    }
}