using Risen.Contracts.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest req, CancellationToken ct);
        Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct);
        Task<AuthResponse> RefreshAsync(RefreshRequest req, CancellationToken ct);
        Task LogoutAsync(LogoutRequest req, CancellationToken ct);
        Task<string?> SendForgotPasswordAsync(ForgotPasswordRequest req, CancellationToken ct);
        Task<Risen.Contracts.Auth.AuthResponse?> ResetPasswordAsync(ResetPasswordRequest req, CancellationToken ct);
    }
}
