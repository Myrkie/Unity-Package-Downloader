using System.Security.Cryptography;

namespace Unity_package_downloader.Decryption
{
    public static class Decryption
    {
        public static async Task DecryptString(string inputFile, string outputFile, IEnumerable<byte> key, byte[] iv)
        {
            var encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            encryptor.Key = key.Take(32).ToArray();
            encryptor.IV = iv;

            var fileStream = File.OpenWrite(outputFile);

            var instream = File.OpenRead(inputFile);

            var aesDecryptor = encryptor.CreateDecryptor();

            var cryptoStream = new CryptoStream(fileStream, aesDecryptor, CryptoStreamMode.Write);

            try
            {
                await instream.CopyToAsync(cryptoStream);

                await cryptoStream.FlushFinalBlockAsync();
            }
            finally
            {
                instream.Close();
                fileStream.Close();
                cryptoStream.Close();
            }
        }
    }
}