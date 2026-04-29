using System;

namespace Risen.Business.Options
{
    public sealed class EmailOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; } = false;
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string From { get; set; } = "no-reply@risen.local";
        public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
    }
}
