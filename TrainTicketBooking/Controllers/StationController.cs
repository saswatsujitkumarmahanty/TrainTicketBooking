using Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketBooking.Controllers
{ 
[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IStationRepository _repo;
    public StationsController(IStationRepository repo) => _repo = repo;

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term)
        => Ok(await _repo.SearchAsync(term));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var station = await _repo.GetByIdAsync(id);
        return station is null ? NotFound() : Ok(station);
    }
}
}