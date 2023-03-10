using System.Security.Cryptography;

namespace Unity_package_downloader.Decryption;

public class Decrpytion
{

    public static async Task DecryptString(string inputFile, string outputFile, IEnumerable<byte> key, byte[] iv)
    {
        // Instantiate a new Aes object to perform string symmetric encryption
        var encryptor = Aes.Create();

        encryptor.Mode = CipherMode.CBC;
        //encryptor.KeySize = 256;
        //encryptor.BlockSize = 128;
        //encryptor.Padding = PaddingMode.Zeros;

        // Set key and IV
        encryptor.Key = key.Take(32).ToArray();
        encryptor.IV = iv;

        // Instantiate a new MemoryStream object to contain the encrypted bytes
        var fileStream = File.OpenWrite(outputFile);

        var instream = File.OpenRead(inputFile);

        // Instantiate a new encryptor from our Aes object
        var aesDecryptor = encryptor.CreateDecryptor();

        // Instantiate a new CryptoStream object to process the data and write it to the 
        // memory stream
        var cryptoStream = new CryptoStream(fileStream, aesDecryptor, CryptoStreamMode.Write);

        try
        {
            instream.CopyToAsync(cryptoStream);

            // Complete the decryption process
            cryptoStream.FlushFinalBlockAsync();
        }
        finally
        {
            // Close both the MemoryStream and the CryptoStream
            instream.Close();
            fileStream.Close();
            cryptoStream.Close();
        }
    }
    
}