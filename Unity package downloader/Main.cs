using Unity_package_downloader;

class aes
{
    // todo: Oath shit
    // todo: add thumbnail downloader
    static async Task Main(string[] args)
    {
        await WebRequests.loop();
        Thread.Sleep(5000);
    }
}