using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicAggregator.Application.Abstractions;
using static MusicAggregator.Application.Abstractions.IApiStatsStore;

namespace MusicAggregator.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IApiStatsStore _store;
        public StatsController(IApiStatsStore store)
        {
            _store = store;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ApiStatsSnapshot>), StatusCodes.Status200OK)]
        public ActionResult<IReadOnlyList<ApiStatsSnapshot>> Get()
        {
            return Ok(_store.GetSnapshot());
        }
    }
}
