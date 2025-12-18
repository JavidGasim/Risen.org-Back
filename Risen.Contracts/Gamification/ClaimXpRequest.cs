using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Gamification
{
    public sealed record ClaimXpRequest(
      string SourceKey,          // e.g. "Quest:9f...-attempt:1"
      int BaseXp,                // məsələn 50
      decimal DifficultyMultiplier // məsələn 1.0 / 1.5 / 2.0
  );
}
