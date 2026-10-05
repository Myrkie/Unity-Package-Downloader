using System.Security.Cryptography;

namespace Unity_package_downloader.Decryption
{
    public static class Decryptor
    {
        public static async Task DecryptFile(string inputFile, string outputFile, IEnumerable<byte> key, byte[] iv)
        {
            FileInfo inputInfo = new FileInfo(inputFile);
            long totalBytes = inputInfo.Length;
            long processedBytes = 0;
            using Aes encryptor = Aes.Create();
            encryptor.Mode = CipherMode.CBC;
            encryptor.Key = [.. key.Take(32)];
            encryptor.IV = iv;
            using ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();
            await using FileStream inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: 81920, useAsync: true);
            await using FileStream outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write,
                FileShare.None, bufferSize: 81920, useAsync: true);
            await using CryptoStream cryptoStream = new CryptoStream(outputStream, aesDecryptor, CryptoStreamMode.Write);
            
            DownloadProgressUi progressUi = new DownloadProgressUi("Decrypting");
            byte[] buffer = new byte[81920];
            progressUi.DrawProgress(processedBytes, totalBytes);
            int bytesRead;
            while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await cryptoStream.WriteAsync(buffer, 0, bytesRead);
                processedBytes += bytesRead;
                progressUi.DrawProgress(processedBytes, totalBytes);
            }

            await cryptoStream.FlushFinalBlockAsync();
            if (totalBytes > 0)
            {
                progressUi.DrawProgress(totalBytes, totalBytes);
            }
            
            progressUi.Complete();
        }
    }
}