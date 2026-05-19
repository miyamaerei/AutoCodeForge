using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("Users")]
public class UserEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[SugarTable("RefreshTokens")]
public class RefreshTokenEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
