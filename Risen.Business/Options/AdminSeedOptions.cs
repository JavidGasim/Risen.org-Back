using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Options
{
    public sealed class AdminSeedOptions
    {
        // Production-da yalnız true olanda işləsin
        public bool Enabled { get; set; } = false;

        // Admin user (dev/test üçün)
        public string Email { get; set; } = "admin@risen.local";

        // Repo-da saxlamırıq. Dev-də user-secrets, test/CI-də env var.
        public string? Password { get; set; }

        public string FirstName { get; set; } = "Admin";
        public string LastName { get; set; } = "Risen";
    }
}
