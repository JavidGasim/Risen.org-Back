using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Auth
{
    public sealed record RefreshRequest(string RefreshToken);
}
