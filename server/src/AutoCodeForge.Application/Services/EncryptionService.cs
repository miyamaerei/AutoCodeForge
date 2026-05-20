using System.Security.Cryptography;
using System.Text;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides AES-256 encryption and decryption for sensitive configuration values.
/// </summary>
public class EncryptionService
{
    private const int KeySize = 256;
    private const int BlockSize = 128;
    private const int SaltSize = 16;
    private const int Iterations = 10000;

    private readonly byte[] _encryptionKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptionService"/> class.
    /// </summary>
    /// <param name="encryptionKey">The base64-encoded encryption key.</param>
    public EncryptionService(string encryptionKey)
    {
        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            throw new ArgumentException("Encryption key cannot be null or empty.", nameof(encryptionKey));
        }

        _encryptionKey = Convert.FromBase64String(encryptionKey);
        if (_encryptionKey.Length != KeySize / 8)
        {
            throw new ArgumentException($"Encryption key must be {KeySize} bits (32 bytes) when decoded.", nameof(encryptionKey));
        }
    }

    /// <summary>
    /// Encrypts a plain text string using AES-256.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <returns>The encrypted string in base64 format.</returns>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return plainText;
        }

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        var key = DeriveKey(salt);
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        cryptoStream.FlushFinalBlock();

        var encryptedBytes = memoryStream.ToArray();
        var result = new byte[salt.Length + aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(aes.IV, 0, result, salt.Length, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, salt.Length + aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts a base64-encoded encrypted string.
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt.</param>
    /// <returns>The decrypted plain text.</returns>
    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
        {
            return encryptedText;
        }

        var fullBytes = Convert.FromBase64String(encryptedText);

        var salt = new byte[SaltSize];
        var iv = new byte[BlockSize / 8];
        var encryptedBytes = new byte[fullBytes.Length - salt.Length - iv.Length];

        Buffer.BlockCopy(fullBytes, 0, salt, 0, salt.Length);
        Buffer.BlockCopy(fullBytes, salt.Length, iv, 0, iv.Length);
        Buffer.BlockCopy(fullBytes, salt.Length + iv.Length, encryptedBytes, 0, encryptedBytes.Length);

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var key = DeriveKey(salt);
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream(encryptedBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream, Encoding.UTF8);

        return streamReader.ReadToEnd();
    }

    private byte[] DeriveKey(byte[] salt)
    {
        // Use the static Pbkdf2 overload that takes HashAlgorithmName then the desired length
        return Rfc2898DeriveBytes.Pbkdf2(_encryptionKey, salt, Iterations, HashAlgorithmName.SHA256, KeySize / 8);
    }

    /// <summary>
    /// Generates a new encryption key.
    /// </summary>
    /// <returns>A base64-encoded 256-bit encryption key.</returns>
    public static string GenerateKey()
    {
        var key = new byte[KeySize / 8];
        RandomNumberGenerator.Fill(key);
        return Convert.ToBase64String(key);
    }
}