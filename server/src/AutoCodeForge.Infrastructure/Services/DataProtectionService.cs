using Microsoft.AspNetCore.DataProtection;

namespace AutoCodeForge.Infrastructure.Services;

/// <summary>
/// Provides encryption and decryption services for sensitive data.
/// </summary>
public class DataProtectionService
{
    private readonly IDataProtector _dataProtector;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataProtectionService"/> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    public DataProtectionService(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("AutoCodeForge.Credentials");
    }

    /// <summary>
    /// Encrypts plain text data.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted data.</returns>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return plainText;
        }

        return _dataProtector.Protect(plainText);
    }

    /// <summary>
    /// Decrypts encrypted data.
    /// </summary>
    /// <param name="cipherText">The encrypted data to decrypt.</param>
    /// <returns>The decrypted plain text.</returns>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            return cipherText;
        }

        try
        {
            return _dataProtector.Unprotect(cipherText);
        }
        catch
        {
            // If decryption fails (e.g., key not available), return empty string
            return string.Empty;
        }
    }
}
