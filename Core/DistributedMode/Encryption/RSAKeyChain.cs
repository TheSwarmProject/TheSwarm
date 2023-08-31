using System.Security.Cryptography;
using System.Text;

namespace TheSwarm.Components.Distributed.Encryption;

/// <summary>
/// Convenience type - generates own key pair and stores the public key received from the other node.
/// Also handles encryption/descryption procedures
/// </summary>
public class RSAKeyChain
{
    public RSACryptoServiceProvider     MyRSA       { get; init; }
    public RSACryptoServiceProvider?    OtherRSA    { get; private set; }

    public RSAKeyChain()
    {
        MyRSA = new RSACryptoServiceProvider();
    }

    /// <summary>
    /// Receives and sets other's public key and returns our key - which is to be sent to other holder
    /// </summary>
    /// <param name="otherKey">Public key from other entity</param>
    /// <returns>Our public key</returns>
    public string ExchangePublicKeys(string otherKey)
    {
        OtherRSA = new RSACryptoServiceProvider();
        OtherRSA.FromXmlString(otherKey);

        return MyRSA.ToXmlString(false);
    }

    /// <summary>
    /// Encrypts the data using other's public key
    /// </summary>
    /// <param name="data">Data to encrypt</param>
    /// <returns>Result byte array</returns>
    public byte[] Encrypt(string data)
    {
        if (OtherRSA is null)
            throw new Exception("RSA provider for other side of the channel was not initialized.");

        return OtherRSA.Encrypt(new UnicodeEncoding().GetBytes(data), false);
    }

    /// <summary>
    /// Encrypts the data using other's public key, but with byte array instead of string
    /// </summary>
    /// <param name="data">Byte array to encrypt</param>
    /// <returns>Result byte array</returns>
    public byte[] Encrypt(byte[] data)
    {
        if (OtherRSA is null)
            throw new Exception("RSA provider for other side of the channel was not initialized.");

        return OtherRSA.Encrypt(data, false);
    }

    /// <summary>
    /// Decrypts the data using our private RSA key
    /// </summary>
    /// <param name="data">Data to decrypt</param>
    /// <returns>Result string</returns>
    public string Decrypt(byte[] data)
    {
        return new UnicodeEncoding().GetString(MyRSA.Decrypt(data, false));
    }

    /// <summary>
    /// Decrypts the data using our private RSA key, but returns the byte array instead of string
    /// </summary>
    /// <param name="data">Data to decrypt</param>
    /// <returns>Result byte array</returns>
    public byte[] DecryptAsByteArray(byte[] data)
    {
        return MyRSA.Decrypt(data, false);
    }
}