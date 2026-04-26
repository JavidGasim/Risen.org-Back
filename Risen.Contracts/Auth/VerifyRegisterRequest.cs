using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Auth
{
    public class VerifyRegisterRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
