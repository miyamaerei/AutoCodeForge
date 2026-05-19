using System.Text.Json.Serialization;

namespace __ProjectName__.Api;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = "Success";
    public T? Data { get; set; }
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T> { Code = 0, Data = data };
    }

    public static ApiResponse<T> Success(string message, T data)
    {
        return new ApiResponse<T> { Code = 0, Message = message, Data = data };
    }

    public static ApiResponse<T> Error(int code, string message)
    {
        return new ApiResponse<T> { Code = code, Message = message };
    }
}

public class ApiErrorResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public string Path { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public IDictionary<string, string[]>? Errors { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string NtId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class LoginRequest
{
    [JsonPropertyName("ntId")]
    public string NtId { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [JsonPropertyName("ntId")]
    public string NtId { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
}
