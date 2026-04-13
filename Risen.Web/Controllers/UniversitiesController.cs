using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Risen.Business.Integrations.Hipolabs;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Universities;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {
        private readonly IUniversitySuggestService _svc;
        private readonly ILogger<UniversitiesController> _logger;
        private readonly IUniversityCandidateService _candidates;


        public UniversitiesController(IUniversitySuggestService svc, ILogger<UniversitiesController> logger, IUniversityCandidateService candidates)
        {
            _svc = svc;
            _logger = logger;
            _candidates = candidates;
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

        [Authorize(Roles = "Admin,University")]
        [HttpGet("candidates")]
        public async Task<ActionResult<CandidatesResponse>> GetCandidates(
        [FromQuery] string? minLeague = null,
        [FromQuery] decimal? minScore = null,
        [FromQuery] string? country = null,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        CancellationToken ct = default)
        {
            LeagueCode? leagueCode = null;
            if (!string.IsNullOrWhiteSpace(minLeague))
            {
                if (!Enum.TryParse<LeagueCode>(minLeague, ignoreCase: true, out var parsed))
                {
                    _logger.LogWarning("Invalid league code provided: {MinLeague}", minLeague);
                    return BadRequest(new { message = $"Etibarsız league: {minLeague}" });
                }

                leagueCode = parsed;
            }

            var result = await _candidates.GetCandidatesAsync(
                leagueCode, minScore, country, limit, offset, ct);

            _logger.LogInformation("Retrieved {Count} candidates with minLeague: {MinLeague}, minScore: {MinScore}, country: {Country}, limit: {Limit}, offset: {Offset}",
                result.Items.Count, minLeague, minScore, country, limit, offset);

            return Ok(result);
        }
    }
}

