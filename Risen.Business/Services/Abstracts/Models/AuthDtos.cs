namespace Risen.Business.Services.Abstracts.Models;

public record RegisterRequest(string Email, string Password, string FullName, string? Country);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string Plan,
    bool IsPremium
);