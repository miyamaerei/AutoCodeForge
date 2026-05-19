namespace __ProjectName__.Configuration;

public class AppSettings
{
    public JwtSettings JwtSettings { get; set; } = new();
    public ApiSettings ApiSettings { get; set; } = new();
}

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 120;
    public int RefreshTokenExpiresInDays { get; set; } = 7;
}

public class ApiSettings
{
    public int RequestTimeoutSeconds { get; set; } = 30;
    public int MaxRequestSizeMb { get; set; } = 10;
}
