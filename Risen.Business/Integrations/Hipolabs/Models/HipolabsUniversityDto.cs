using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Risen.Business.Integrations.Hipolabs.Models
{
    public class HipolabsUniversityDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("country")]
        public string Country { get; set; } = default!;

        [JsonPropertyName("state-province")]
        public string? StateProvince { get; set; }

        [JsonPropertyName("alpha_two_code")]
        public string? AlphaTwoCode { get; set; }

        [JsonPropertyName("domains")]
        public List<string>? Domains { get; set; }

        [JsonPropertyName("web_pages")]
        public List<string>? WebPages { get; set; }

        // bəzi variantlarda tək gəlir
        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("web_page")]
        public string? WebPage { get; set; }
    }
}
