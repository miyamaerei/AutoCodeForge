using AutoCodeForge.Application.Services;

namespace AutoCodeForge.Tests;

public sealed class EncryptionServiceTests : IDisposable
{
    private readonly string _testEncryptionKey;
    private readonly string _invalidLengthKey;
    private readonly EncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        _testEncryptionKey = EncryptionService.GenerateKey();
        _encryptionService = new EncryptionService(_testEncryptionKey);
        
        // Create an invalid key (not 32 bytes when decoded)
        _invalidLengthKey = Convert.ToBase64String(new byte[16]);
    }

    [Fact]
    public void GenerateKey_ShouldReturnValidBase64Key()
    {
        var key = EncryptionService.GenerateKey();
        Assert.False(string.IsNullOrWhiteSpace(key));
        
        // Verify it can be decoded to 32 bytes
        var decoded = Convert.FromBase64String(key);
        Assert.Equal(32, decoded.Length);
    }

    [Fact]
    public void Constructor_WithValidKey_ShouldCreateService()
    {
        var service = new EncryptionService(_testEncryptionKey);
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullKey_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => new EncryptionService(null!));
        Assert.Equal("encryptionKey", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyKey_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => new EncryptionService(string.Empty));
        Assert.Equal("encryptionKey", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidLengthKey_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => new EncryptionService(_invalidLengthKey));
        Assert.Equal("encryptionKey", exception.ParamName);
    }

    [Fact]
    public void Encrypt_WithNullInput_ShouldReturnNull()
    {
        var result = _encryptionService.Encrypt(null!);
        Assert.Null(result);
    }

    [Fact]
    public void Encrypt_WithEmptyInput_ShouldReturnEmpty()
    {
        var result = _encryptionService.Encrypt(string.Empty);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Encrypt_WithValidInput_ShouldReturnEncryptedString()
    {
        var plainText = "This is a test message for encryption";
        var encrypted = _encryptionService.Encrypt(plainText);
        
        Assert.False(string.IsNullOrWhiteSpace(encrypted));
        Assert.NotEqual(plainText, encrypted);
    }

    [Fact]
    public void Decrypt_WithNullInput_ShouldReturnNull()
    {
        var result = _encryptionService.Decrypt(null!);
        Assert.Null(result);
    }

    [Fact]
    public void Decrypt_WithEmptyInput_ShouldReturnEmpty()
    {
        var result = _encryptionService.Decrypt(string.Empty);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Decrypt_WithEncryptedInput_ShouldReturnOriginalPlainText()
    {
        var plainText = "This is a test message for round-trip encryption";
        var encrypted = _encryptionService.Encrypt(plainText);
        var decrypted = _encryptionService.Decrypt(encrypted);
        
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        var plainText = "!@#$%^&*()_+{}[]|\\:;\"'<>?,./~`";
        var encrypted = _encryptionService.Encrypt(plainText);
        var decrypted = _encryptionService.Decrypt(encrypted);
        
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_WithUnicodeCharacters_ShouldWorkCorrectly()
    {
        var plainText = "你好世界 🌍 Привет мир مرحباً بالعالم";
        var encrypted = _encryptionService.Encrypt(plainText);
        var decrypted = _encryptionService.Decrypt(encrypted);
        
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_WithSameInputMultipleTimes_ShouldProduceDifferentOutputs()
    {
        // Due to random salt and IV, same input should produce different outputs
        var plainText = "Same input, different outputs";
        var encrypted1 = _encryptionService.Encrypt(plainText);
        var encrypted2 = _encryptionService.Encrypt(plainText);
        
        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void Decrypt_WithDifferentEncryptionsOfSameInput_ShouldProduceSameOriginal()
    {
        var plainText = "Different encryptions, same original";
        var encrypted1 = _encryptionService.Encrypt(plainText);
        var encrypted2 = _encryptionService.Encrypt(plainText);
        var decrypted1 = _encryptionService.Decrypt(encrypted1);
        var decrypted2 = _encryptionService.Decrypt(encrypted2);
        
        Assert.Equal(plainText, decrypted1);
        Assert.Equal(plainText, decrypted2);
    }

    public void Dispose()
    {
        // Nothing to dispose for this test
    }
}
