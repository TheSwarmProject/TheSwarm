using System.Security.Cryptography;
using System.Text;

namespace TheSwarm.Components.Distributed.Encryption;

/// <summary>
/// Convenience type - serves as a main workhorse for encryption/decryption of data transfered between node and hub
/// </summary>
public class AESHandler
{
    private byte[] Key   { get; init; }
    private byte[] IV    { get; init; }

    /// <summary>
    /// Default constructor - generate the key and IV ourselves
    /// </summary>
    public AESHandler()
    {
        Key = RandomNumberGenerator.GetBytes(32);
        IV = RandomNumberGenerator.GetBytes(16);
    }

    /// <summary>
    /// Initializes handler using existing IV + key byte array
    /// </summary>
    /// <param name="key">Byte array (IV+Key)</param>
    public AESHandler(byte[] key)
    {
        Key = key.Skip(16).Take(32).ToArray();
        IV = key.Skip(0).Take(16).ToArray();
    }

    /// <summary>
    /// Combines Key with IV into a single byte array (IV+Key) and returns it
    /// </summary>
    /// <returns>IV+Key array</returns>
    public byte[] GetKey()
    {
        return IV.Concat(Key).ToArray();
    }

    /// <summary>
    /// Encrypts provided string data
    /// </summary>
    /// <param name="data">String data</param>
    /// <returns>Encrypted byte array</returns>
    public byte[] Encrypt(string data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;
            var symmetricEncryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream as Stream, symmetricEncryptor, CryptoStreamMode.Write))
                {
                    using (var streamWriter = new StreamWriter(cryptoStream as Stream))
                        streamWriter.Write(data);
                    
                    return memoryStream.ToArray();
                }
        }
    }

    /// <summary>
    /// Encrypts provided byte array
    /// </summary>
    /// <param name="data">Byte array</param>
    /// <returns>Encrypted byte array</returns>
    public byte[] Encrypt(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;
            var symmetricEncryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream as Stream, symmetricEncryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
        }
    }

    /// <summary>
    /// Decrypts provided byte array into a string
    /// </summary>
    /// <param name="data">Byte array</param>
    /// <returns>Decrypted string</returns>
    public string Decrypt(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (var memoryStream = new MemoryStream(data))
                using (var cryptoStream = new CryptoStream(memoryStream as Stream, decryptor, CryptoStreamMode.Read))
                    using (var streamReader = new StreamReader(cryptoStream as Stream))
                        return streamReader.ReadToEnd();
        }
    }

    /// <summary>
    /// Decrypts provided byte array into a decrypted byte array
    /// </summary>
    /// <param name="data">Byte array</param>
    /// <returns>Decrypted byte array</returns>
    public byte[] DecryptAsByteArray(byte[] data)
    {
        byte[] decrypted = new byte[data.Length];
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (var memoryStream = new MemoryStream(data))
                using (var cryptoStream = new CryptoStream(memoryStream as Stream, decryptor, CryptoStreamMode.Read))
                {
                    int bytesRead = cryptoStream.Read(decrypted, 0, data.Length);
                    return decrypted.Take(bytesRead).ToArray();
                }
        }
    }
}