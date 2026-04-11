using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Risen.Business.Integrations.Hipolabs;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Universities;
using Risen.DataAccess.Data;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {
        private readonly IUniversitySuggestService _svc;
        private readonly ILogger<UniversitiesController> _logger;

        public UniversitiesController(IUniversitySuggestService svc, ILogger<UniversitiesController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        // GET /api/universities/suggest?q=aze&limit=10
        [HttpGet("suggest")]
        public async Task<ActionResult<string[]>> Suggest(
            [FromQuery] string q,
            [FromQuery] int limit = 10,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Received suggest request with query: {Query} and limit: {Limit}", q, limit);
            return Ok(await _svc.SuggestAsync(q, limit, ct));
        }

        // GET /api/universities/search?q=azerbaijan&country=Azerbaijan&limit=20&offset=0
        [HttpGet("search")]
        public async Task<ActionResult<UniversitySearchResponseDto>> Search(
            [FromQuery] string q,
            [FromQuery] string? country = null,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Received search request with query: {Query}, country: {Country}, limit: {Limit}, offset: {Offset}", q, country, limit, offset);
            return Ok(await _svc.SearchAsync(q, country, limit, offset, ct));
        }
    }

}

