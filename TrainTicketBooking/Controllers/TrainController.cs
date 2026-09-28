using Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainsController(ITrainRepository repo) : ControllerBase
    {
        // GET api/trains/search?from=1&to=3&date=2026-10-05
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] int from, [FromQuery] int to, [FromQuery] DateOnly date)
        {
            if (from == to)
                return BadRequest("Source and destination must be different.");
            if (date < DateOnly.FromDateTime(DateTime.Today))
                return BadRequest("Journey date cannot be in the past.");

            return Ok(await repo.SearchAsync(from, to, date));
        }
    }
}
