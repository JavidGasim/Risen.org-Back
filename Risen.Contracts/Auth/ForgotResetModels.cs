using System;

namespace Risen.Contracts.Auth
{
    public sealed record ForgotPasswordRequest(string Email);

    public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
}
