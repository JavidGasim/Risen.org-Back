using Risen.Business.Integrations.Hipolabs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Integrations.Hipolabs
{
    public class HipolabsClient : IHipolabsClient
    {
        private readonly HttpClient _http;

        public HipolabsClient(HttpClient http) => _http = http;

        public async Task<List<HipolabsUniversityDto>> SearchAsync(string? name, string? country, int limit, int offset, CancellationToken ct)
        {
            limit = Math.Clamp(limit, 1, 100);
            offset = Math.Max(0, offset);

            // Hipolabs: /search?name=...&country=...&limit=...&offset=...
            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(name)) qs.Add($"name={Uri.EscapeDataString(name)}");
            if (!string.IsNullOrWhiteSpace(country)) qs.Add($"country={Uri.EscapeDataString(country)}");
            qs.Add($"limit={limit}");
            qs.Add($"offset={offset}");

            var url = "/search?" + string.Join("&", qs);

            var result = await _http.GetFromJsonAsync<List<HipolabsUniversityDto>>(url, ct);
            return result ?? new List<HipolabsUniversityDto>();
        }
    }
}
