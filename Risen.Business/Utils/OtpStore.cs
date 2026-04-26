using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Utils
{
    public static class OtpStore
    {
        public static Dictionary<string, (Risen.Contracts.Auth.RegisterRequest Req, string Code, DateTime Expire)> Data
            = new();
    }
}
