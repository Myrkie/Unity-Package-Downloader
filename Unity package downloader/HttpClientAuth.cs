using System.Net.Http.Headers;
using Serilog;

namespace Unity_package_downloader
{
    public class HttpClientAuth
    {
        private static readonly ILogger Logger = Log.ForContext<HttpClientAuth>();
        protected HttpClientAuth()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }

        protected void SetBearerToken(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        protected HttpClient client { get; }

        protected async Task DownloadFile(string address, string filename)
        {
            Logger.ForContext("NoNewLine", true).Information("Downloading File: {Address}", address);

            await Console.Out.FlushAsync();

            DownloadProgressUi progressUi = new DownloadProgressUi("Downloading");

            using HttpResponseMessage response = await client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            long downloadedBytes = 0;

            await using Stream contentStream = await response.Content.ReadAsStreamAsync();

            await using FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 81920, useAsync: true);

            byte[] buffer = new byte[81920];
            progressUi.DrawProgress(downloadedBytes, totalBytes);
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                downloadedBytes += bytesRead;
                progressUi.DrawProgress(downloadedBytes, totalBytes);
            }

            if (totalBytes > 0)
            {
                progressUi.DrawProgress(totalBytes, totalBytes);
            }
            progressUi.Complete();
        }
        
        protected async Task DownloadImage(string address, string filename)
        {
            client.DefaultRequestHeaders.Authorization = null;
            using HttpResponseMessage response = await client.GetAsync(address);
            response.EnsureSuccessStatusCode();

            await using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            await contentStream.CopyToAsync(fileStream);
        }
    }
}