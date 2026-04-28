using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Subjects;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _svc;
        private readonly ILogger<SubjectsController> _logger;

        public SubjectsController(ISubjectService svc, ILogger<SubjectsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        // GET /api/subjects?includeInactive=false
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false, CancellationToken ct = default)
        {
            var items = await _svc.GetAllAsync(includeInactive, ct);
            return Ok(items);
        }
    }
}
