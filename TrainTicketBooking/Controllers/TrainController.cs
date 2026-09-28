using Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainsController(ITrainRepository repo) : ControllerBase
    {
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] int from, [FromQuery] int to, [FromQuery] DateOnly date)
        {
            if (from == to)
                return BadRequest("Source and destination must be different.");
            if (date < DateOnly.FromDateTime(DateTime.Today))
                return BadRequest("Journey date cannot be in the past.");

            return Ok(await repo.SearchAsync(from, to, date));
        }
        
        [HttpGet("vacancy")]
        public async Task<IActionResult> Vacancy(
            [FromQuery] string trainNumber, [FromQuery] int from, [FromQuery] int to, [FromQuery] DateOnly date)
        {
            if (string.IsNullOrWhiteSpace(trainNumber))
                return BadRequest(new { error = "Train number is required." });
            if (from == to)
                return BadRequest(new { error = "Source and destination must be different." });
            if (date < DateOnly.FromDateTime(DateTime.Today))
                return BadRequest(new { error = "Journey date cannot be in the past." });

            var trains = await repo.SearchAsync(from, to, date);
            var train = trains.FirstOrDefault(t =>
                string.Equals(t.TrainNumber.Trim(), trainNumber.Trim(), StringComparison.OrdinalIgnoreCase));

            return train is null
                ? NotFound(new { error = $"Train {trainNumber.Trim()} does not run between these stations on this date." })
                : Ok(train);
        }
    }
}